using App.ViewModels;
using Avalonia.Collections;
using Avalonia.Controls;
using log4net;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UgCSPPK.Models;
using UgCSPPK.Models.Yaml;
using YamlDotNet.Serialization;

namespace UgCSPPK.ViewModels
{
    public class PpkToolViewModel : ViewModelBase
    {
        private const string PositioningSolutionFilesTemplatesFolder = "./Mapping/PSFTemplates";
        private const string FilesToUpdateTemplatesFolder = "./Mapping/FTUTemplates";
        private string lastOpenedFolder = "";
        private bool isDialogOpen = false;
        private Deserializer deserializer = new Deserializer();
        private ObservableCollection<PositioningSolutionFile> positioningSolutionFiles = new ObservableCollection<PositioningSolutionFile>();
        private ObservableCollection<FileToUpdate> filesToUpdate = new ObservableCollection<FileToUpdate>();
        private List<Template> psfTemplates = new List<Template>();
        private List<Template> ftuTemplates = new List<Template>();

        public DataGridCollectionView FilesToUpdate { get; }

        public DataGridCollectionView Data { get; }

        private PositioningSolutionFile _selectedPositioningSolutionFile;

        public PositioningSolutionFile SelectedPositioningSolutionFile
        {
            get => _selectedPositioningSolutionFile;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedPositioningSolutionFile, value);
            }
        }

        private FileToUpdate _selectedFileToUpdate;

        public FileToUpdate SelectedFileToUpdate
        {
            get => _selectedFileToUpdate;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedFileToUpdate, value);
            }
        }

        public PpkToolViewModel()
        {
            Data = new DataGridCollectionView(positioningSolutionFiles);
            FilesToUpdate = new DataGridCollectionView(filesToUpdate);
            CreateTemplates();
        }

        private async void AddPositioningSolutionFile()
        {
            if (isDialogOpen)
                return;
            isDialogOpen = true;
            OpenFileDialog openDialog = new OpenFileDialog() { AllowMultiple = true, Directory = lastOpenedFolder };
            openDialog.Filters.Add(new FileDialogFilter() { Name = "PPK Log", Extensions = { "pos" } });
            var chosenFiles = await openDialog.ShowAsync(new Window());
            if (chosenFiles != null)
            {
                foreach (var file in chosenFiles)
                {
                    var template = FindTemplate(file);
                    if (template != null)
                    {
                        var psf = new PositioningSolutionFile(file, template);
                        positioningSolutionFiles.Add(psf);
                        foreach (var f in filesToUpdate)
                        {
                            f.CheckCoveringStatus(positioningSolutionFiles.ToList());
                        }
                    }
                }
            }
            isDialogOpen = false;
        }

        private void RemovePositioningSolutionFile()
        {
            if (Data.Contains(SelectedPositioningSolutionFile))
            {
                foreach (var f in filesToUpdate)
                    if (f.CoverageFile == SelectedPositioningSolutionFile)
                        f.UnsetCoverageFile();
                Data.Remove(SelectedPositioningSolutionFile);
            }          
        }

        private async void AddFileToUpdate()
        {
            if (isDialogOpen)
                return;
            isDialogOpen = true;
            OpenFileDialog openDialog = new OpenFileDialog() { AllowMultiple = true, Directory = lastOpenedFolder };
            openDialog.Filters.Add(new FileDialogFilter() { Name = "Position Log", Extensions = { "csv" } });
            var chosenFiles = await openDialog.ShowAsync(new Window());
            if (chosenFiles != null)
            {
                foreach (var file in chosenFiles)
                {
                    var template = FindTemplate(file);
                    if (template != null)
                    {
                        var ftu = new FileToUpdate(file, template);
                        filesToUpdate.Add(ftu);
                        foreach (var f in filesToUpdate)
                        {
                            f.CheckCoveringStatus(positioningSolutionFiles.ToList());
                        }
                    }
                }
            }
            isDialogOpen = false;
        }

        private void RemoveFileToUpdate()
        {
            if (FilesToUpdate.Contains(SelectedFileToUpdate))
                FilesToUpdate.Remove(SelectedFileToUpdate);
        }

        private async void BrowseFolder()
        {
            if (isDialogOpen)
                return;
            isDialogOpen = true;
            OpenFolderDialog openDialog = new OpenFolderDialog
            {
                Directory = ""
            };
            var folder = await openDialog.ShowAsync(new Window());
            if (folder != null)
            {
                try
                {
                    var files = Directory.GetFiles(folder);
                    foreach (var file in files)
                    {
                        var template = FindTemplate(file);
                        if (template?.FileType == FileType.ColumnsFixedWidth)
                        {
                            var psf = new PositioningSolutionFile(file, template);
                            positioningSolutionFiles.Add(psf);
                        }
                        else if (template?.FileType == FileType.CSV)
                        {
                            var ftu = new FileToUpdate(file, template);
                            filesToUpdate.Add(ftu);
                        }
                        foreach (var f in filesToUpdate)
                        {
                            f.CheckCoveringStatus(positioningSolutionFiles.ToList());
                        }
                    }
                }
                catch (Exception)
                {
                }
            }
            isDialogOpen = false;
        }

        private void CreateTemplates()
        {
            if (!Directory.Exists(PositioningSolutionFilesTemplatesFolder))
                return;
            var files = Directory.GetFiles(PositioningSolutionFilesTemplatesFolder, "*.yaml");
            foreach (var file in files)
            {
                try
                {
                    var data = File.ReadAllText(file);
                    var tempalte = deserializer.Deserialize<Template>(data);
                    psfTemplates.Add(tempalte);
                }
                catch (Exception)
                {
                }
            }

            if (!Directory.Exists(FilesToUpdateTemplatesFolder))
                return;
            files = Directory.GetFiles(FilesToUpdateTemplatesFolder, "*.yaml");
            foreach (var file in files)
            {
                try
                {
                    var data = File.ReadAllText(file);
                    var tempalte = deserializer.Deserialize<Template>(data);
                    ftuTemplates.Add(tempalte);
                }
                catch (Exception)
                {
                }
            }
        }

        private Template FindTemplate(string file)
        {
            foreach (var t in psfTemplates.Union(ftuTemplates))
            {
                var firstNonEmptyLines = File.ReadLines(file).Take(10).ToList();
                foreach (var l in firstNonEmptyLines)
                {
                    var regex = new Regex(t.MatchRegex);
                    if (regex.IsMatch(l))
                        return t;
                }
            }
            return null;
        }

        private void ProcessFiles()
        {
            foreach (var ftu in filesToUpdate)
            {
                ftu.UpdateCoordinates();
            }
        }
    }
}