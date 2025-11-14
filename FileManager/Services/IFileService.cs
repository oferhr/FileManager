using System.Collections.Generic;

namespace FileManager.Services
{
    public interface IFileService
    {
        bool IsThumbsInPath(string filePath);
        void CopyFiles(string src, string dest);
        void MoveFiles(string src, string dest);
        string GetNewFileName(string name, int num);
        string[] GetParts(string file);
        string GetMailFileName(string fileName, int isCheck, bool shortenName = false);
    }
}
