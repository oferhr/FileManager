using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace FileManager.Services
{
    /// <summary>
    /// Loads <c>actions.json</c> once at construction and exposes case-insensitive lookups.
    /// </summary>
    public class ActionConfigService : IActionConfigService
    {
        private const string ConfigFileName = "actions.json";

        private readonly ILoggingService _loggingService;
        private readonly Dictionary<string, bool> _actionStates =
            new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

        public ActionConfigService(ILoggingService loggingService)
        {
            _loggingService = loggingService;
            LoadConfig();
        }

        public bool IsActionEnabled(string actionName)
        {
            if (string.IsNullOrWhiteSpace(actionName))
            {
                return true;
            }

            if (_actionStates.TryGetValue(actionName, out var enabled))
            {
                return enabled;
            }

            // Key missing → fail-open (action allowed by default)
            return true;
        }

        private void LoadConfig()
        {
            string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigFileName);

            if (!File.Exists(configPath))
            {
                _loggingService?.LogInfo(
                    $"{ConfigFileName} not found at '{configPath}'. All actions allowed by default.",
                    "ActionConfigService");
                return;
            }

            try
            {
                var json = File.ReadAllText(configPath);
                var parsed = JObject.Parse(json);

                var knownActions = new HashSet<string>(ActionNames.All, StringComparer.OrdinalIgnoreCase);

                foreach (var property in parsed.Properties())
                {
                    _actionStates[property.Name] = ParseBool(property.Name, property.Value);

                    if (!knownActions.Contains(property.Name))
                    {
                        _loggingService?.LogWarning(
                            $"{ConfigFileName} contains unknown action key '{property.Name}'. Known actions: {string.Join(", ", ActionNames.All)}",
                            "ActionConfigService");
                    }
                }

                var disabled = _actionStates.Where(kvp => !kvp.Value).Select(kvp => kvp.Key).ToList();

                if (disabled.Count == 0)
                {
                    _loggingService?.LogInfo(
                        $"{ConfigFileName} loaded. All {_actionStates.Count} actions enabled.",
                        "ActionConfigService");
                }
                else
                {
                    _loggingService?.LogInfo(
                        $"{ConfigFileName} loaded. Disabled actions: {string.Join(", ", disabled)}",
                        "ActionConfigService");
                }
            }
            catch (Exception ex)
            {
                _loggingService?.LogError(
                    $"Failed to parse {ConfigFileName}. All actions allowed by default.",
                    ex);
                _actionStates.Clear();
            }
        }

        private bool ParseBool(string key, JToken token)
        {
            if (token == null)
            {
                _loggingService?.LogWarning(
                    $"{ConfigFileName} key '{key}' has null value — treating as enabled.",
                    "ActionConfigService");
                return true;
            }

            switch (token.Type)
            {
                case JTokenType.Boolean:
                    return token.Value<bool>();

                case JTokenType.String:
                    var s = token.Value<string>();
                    if (bool.TryParse(s, out var b))
                    {
                        return b;
                    }
                    _loggingService?.LogWarning(
                        $"{ConfigFileName} key '{key}' has unparseable string value '{s}' — expected 'true' or 'false'. Treating as enabled.",
                        "ActionConfigService");
                    return true;

                default:
                    _loggingService?.LogWarning(
                        $"{ConfigFileName} key '{key}' has unexpected value type '{token.Type}' — expected boolean. Treating as enabled.",
                        "ActionConfigService");
                    return true;
            }
        }
    }
}
