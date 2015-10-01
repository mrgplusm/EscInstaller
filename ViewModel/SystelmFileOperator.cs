using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Timers;
using System.Windows;
using Common;
using EscInstaller.ImportSpeakers;
using Microsoft.Win32;

namespace EscInstaller.ViewModel
{
    public abstract class SystemFileLogic
    {






        public abstract bool Save();


        /// <summary>
        /// Save file without asking to location as defined in LibraryData.SystemFileName
        /// </summary>
        protected void SaveFile()
        {
            if (!LibraryData.SystemIsOpen) return;
            if (string.IsNullOrWhiteSpace(LibraryData.SystemFileName)) return;

            LibraryData.FuturamaSys.LastSavedInstallerVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            try
            {
                FileManagement.SaveSystemFile(LibraryData.FuturamaSys, LibraryData.SystemFileName);

            }
            catch (Exception e)
            {
                MessageBox.Show(string.Format(Main.MessageBoxSaveFileText, e.Message),
                    Main.MessageBoxSaveFileHeader, MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }
    }

    public class SystemFileBackup : SystemFileLogic
    {

        public override bool Save()
        {
            if (!LibraryData.SystemIsOpen) return true;


            try
            {
                File.Delete(LibraryData.SystemFileName + FileManagement.Backupext);

                FileManagement.SaveSystemFile(LibraryData.FuturamaSys,
                    LibraryData.SystemFileName + FileManagement.Backupext);
            }
            catch (Exception e)
            {
                Debug.Write("save backup file failed" + e.Message);
                return false;
            }

            return true;
        }

    }


    public class SystemFileSaveAsk : SystemFileLogic
    {
        /// <summary>
        /// asks user what to do with unsaved data
        /// cancel => false
        /// 
        /// Save file w asking user how to save
        /// </summary>
        /// <returns>true if operation succeded</returns>
        public override bool Save()
        {            
            var option = MessageBox.Show(Main.OverwriteAsk, Main.OverwriteAskTitle, MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question, MessageBoxResult.OK);            

            if (option == MessageBoxResult.No)
                return true;

            if (option == MessageBoxResult.Cancel)
                return false;

            SaveFile();
            return true;
        }
    }

    public class SystemFileSaveAs : SystemFileLogic
    {
        /// <summary>
        /// Save file w asking user how to save
        /// </summary>
        /// <returns>false if user canceled</returns>
        private static bool AskUser()
        {
            var t = FileManagement.SaveFileDialog();
            if (string.IsNullOrWhiteSpace(t.FileName)) return false;

            LibraryData.SystemFileName = t.FileName;
            return true;
        }

        /// <summary>
        /// asks user what to do with unsaved data
        /// </summary>
        public override bool Save()
        {
            if (!AskUser()) return false;
            SaveFile();
            return true;
        }
    }
}