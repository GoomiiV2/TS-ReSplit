using System;
using System.Security.Cryptography;

namespace ModTools
{
    [AttributeUsage(AttributeTargets.Method)]
    public class FileActionAttribute : Attribute
    {
        public string      Name;
        public Platforms   Platform;
        public FileTypes   FileType;
        public EntryTypes  EntryType;
        public ActionTypes ActionType;

        public FileActionAttribute(Platforms Platform, FileTypes FileType, EntryTypes EntryType,
                                   ActionTypes ActionType)
        {
            this.Platform   = Platform;
            this.FileType   = FileType;
            this.EntryType  = EntryType;
            this.ActionType = ActionType;
        }
        public FileActionAttribute(string      Name, Platforms Platform, FileTypes FileType, EntryTypes EntryType,
                                   ActionTypes ActionType)
        {
            this.Name       = Name;
            this.Platform   = Platform;
            this.FileType   = FileType;
            this.EntryType  = EntryType;
            this.ActionType = ActionType;
        }

        public FileActionAttribute(string Name, Platforms Platform, FileTypes FileType, ActionTypes ActionType)
        {
            this.Name       = Name;
            this.Platform   = Platform;
            this.FileType   = FileType;
            EntryType       = EntryTypes.File;
            this.ActionType = ActionType;
        }
        
        public FileActionAttribute(Platforms Platform, FileTypes FileType, ActionTypes ActionType)
        {
            this.Platform   = Platform;
            this.FileType   = FileType;
            EntryType       = EntryTypes.File;
            this.ActionType = ActionType;
        }


        public static int MakeHash(Platforms Platform, FileTypes FileType, EntryTypes EntryType,
                                   ActionTypes ActionType)
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + Platform.GetHashCode();
                hash = hash * 31 + FileType.GetHashCode();
                hash = hash * 31 + EntryType.GetHashCode();
                hash = hash * 31 + ActionType.GetHashCode();
                return hash;
            }
        }

        public enum ActionTypes
        {
            FileAction,
            ListDisplay,
            DoubleClick,
            ContextMenuExtension
        }
    }
}