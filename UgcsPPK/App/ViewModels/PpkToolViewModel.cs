using App.ViewModels;
using Avalonia.Collections;
using Avalonia.Controls;
using FileParsers;
using FileParsers.Yaml;
using log4net;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using UgCSPPK.Models;
using UgCSPPK.Views;
using YamlDotNet.Serialization;

namespace UgCSPPK.ViewModels
{
    public class PpkToolViewModel : ViewModelBase
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(PpkToolViewModel));
        private const string PositioningSolutionFilesTemplatesFolder = "./Mapping/PSFTemplates";
        private const string FilesToUpdateTemplatesFolder = "./Mapping/FTUTemplates";
        private string lastOpenedFolder = "";
        private bool isDialogOpen = false;
        private readonly Deserializer deserializer = new Deserializer();
        private readonly ObservableCollection<PositioningSolutionFile> positioningSolutionFiles = new ObservableCollection<PositioningSolutionFile>();
        private readonly ObservableCollection<FileToUpdate> filesToUpdate = new ObservableCollection<FileToUpdate>();
        private readonly List<Template> psfTemplates = new List<Template>();
        private readonly List<Template> ftuTemplates = new List<Template>();
        private readonly ObservableCollection<string> messages = new ObservableCollection<string>();
        private CancellationTokenSource source;
        private int fileToUpdateTotalLines;
        public DataGridCollectionView FilesToUpdate { get; }

        public DataGridCollectionView PositionSolutionFiles { get; }
        public DataGridCollectionView Messages { get; }

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

        private bool _isProcessFiles = false;

        public bool IsProcessFiles
        {
            get => _isProcessFiles;
            set
            {
                this.RaiseAndSetIfChanged(ref _isProcessFiles, value);
            }
        }

        private double _updatingFileProgressBarValue = 0.00;

        public double UpdatingFileProgressBarValue
        {
            get => _updatingFileProgressBarValue;
            set
            {
                this.RaiseAndSetIfChanged(ref _updatingFileProgressBarValue, value);
            }
        }

        private string _timeOffset = "0";

        public string TimeOffset
        {
            get => _timeOffset;
            set
            {
                this.RaiseAndSetIfChanged(ref _timeOffset, value);
            }
        }


        public PpkToolViewModel()
        {
            PositionSolutionFiles = new DataGridCollectionView(positioningSolutionFiles);
            FilesToUpdate = new DataGridCollectionView(filesToUpdate);
            Messages = new DataGridCollectionView(messages);
            CreateTemplates();
        }

        private async void AddPositioningSolutionFile()
        {
            if (isDialogOpen)
                return;
            isDialogOpen = true;
            OpenFileDialog openDialog = new OpenFileDialog() { AllowMultiple = true, Directory = lastOpenedFolder };
            openDialog.Filters.Add(new FileDialogFilter() { Name = "Position Solution Files", Extensions = { "pos", "csv"} });
            openDialog.Filters.Add(new FileDialogFilter() { Name = "All", Extensions = { "*" } });
            var chosenFiles = await openDialog.ShowAsync(new Window());
            if (chosenFiles != null)
            {
                foreach (var file in chosenFiles)
                {
                    var template = FindTemplate(psfTemplates, file);
                    if (template != null)
                    {
                        var psf = new PositioningSolutionFile(file, template);
                        positioningSolutionFiles.Add(psf);
                        foreach (var f in filesToUpdate)
                        {
                            f.CheckCoveringStatus(positioningSolutionFiles.ToList());
                        }
                    }
                    else
                        messages.Add($"Template for {file} was not found");
                }
                GetLastOpenedDirectory(chosenFiles.FirstOrDefault() ?? "");
            }
            isDialogOpen = false;
        }

        private void RemovePositioningSolutionFile()
        {
            if (PositionSolutionFiles.Contains(SelectedPositioningSolutionFile))
            {
                foreach (var f in filesToUpdate)
                {
                    if (f.CoverageFiles.Contains(SelectedPositioningSolutionFile))
                        f.UnsetCoverageFile(SelectedPositioningSolutionFile);
                }
                PositionSolutionFiles.Remove(SelectedPositioningSolutionFile);
            }
            foreach (var f in filesToUpdate)
            {
                f.CheckCoveringStatus(positioningSolutionFiles.ToList());
            }
        }

        private async void AddFileToUpdate()
        {
            if (isDialogOpen)
                return;
            isDialogOpen = true;
            OpenFileDialog openDialog = new OpenFileDialog() { AllowMultiple = true, Directory = lastOpenedFolder };
            openDialog.Filters.Add(new FileDialogFilter() { Name = "Position Log", Extensions = { "csv", "log" } });
            openDialog.Filters.Add(new FileDialogFilter() { Name = "All", Extensions = { "*" } });
            var chosenFiles = await openDialog.ShowAsync(new Window());
            if (chosenFiles != null)
            {
                foreach (var file in chosenFiles)
                {
                    var template = FindTemplate(ftuTemplates, file);
                    if (template != null)
                    {
                        var ftu = new FileToUpdate(file, template);
                        filesToUpdate.Add(ftu);
                        foreach (var f in filesToUpdate)
                        {
                            f.CheckCoveringStatus(positioningSolutionFiles.ToList());
                        }
                    }
                    else
                        messages.Add($"Template for {file} was not found");
                }
                GetLastOpenedDirectory(chosenFiles.FirstOrDefault() ?? "");
            }
            isDialogOpen = false;
        }

        private void RemoveFileToUpdate()
        {
            if (FilesToUpdate.Contains(SelectedFileToUpdate))
                FilesToUpdate.Remove(SelectedFileToUpdate);
        }

        private void Clear()
        {
            filesToUpdate.Clear();
            positioningSolutionFiles.Clear();
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
            string[] files;
            if (folder != null)
            {
                try
                {
                    files = Directory.GetFiles(folder);
                }
                catch (Exception e)
                {
                    log.Error(e.Message);
                    isDialogOpen = false;
                    return;
                }
                foreach (var file in files)
                {
                    try
                    {
                        var template = FindTemplate(psfTemplates.Union(ftuTemplates).ToList(), file);
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
                        else
                        {
                            messages.Add($"Template for {file} was not found");
                        }
                        foreach (var f in filesToUpdate)
                        {
                            f.CheckCoveringStatus(positioningSolutionFiles.ToList());
                        }
                    }
                    catch (Exception e)
                    {
                        log.Error(e.Message);
                    }
                }
                GetLastOpenedDirectory(folder);
            }
            isDialogOpen = false;
        }

        private void CreateTemplates()
        {
            var nonValidTemplates = new List<string>();
            if (!Directory.Exists(PositioningSolutionFilesTemplatesFolder))
            {
                log.Info($"Directory is not existing: {PositioningSolutionFilesTemplatesFolder[2..]}");
                return;
            }
            string[] files = new string[0];
            try
            {
                files = Directory.GetFiles(PositioningSolutionFilesTemplatesFolder, "*.yaml");
            }
            catch (Exception e)
            {
                log.Error(e.Message);
            }

            foreach (var file in files)
            {
                try
                {
                    var data = File.ReadAllText(file);
                    var tempalte = deserializer.Deserialize<Template>(data);
                    if (tempalte.IsTemplateValid())
                        psfTemplates.Add(tempalte);
                    else
                    {
                        log.Info($"Template is not valid: {file}");
                        nonValidTemplates.Add(file);
                    }
                }
                catch (Exception e)
                {
                    log.Error(e.Message);
                    nonValidTemplates.Add(file);
                }
            }

            if (!Directory.Exists(FilesToUpdateTemplatesFolder))
            {
                log.Info($"Directory is not existing: {FilesToUpdateTemplatesFolder[2..]}");
                return;
            }
            try
            {
                files = Directory.GetFiles(FilesToUpdateTemplatesFolder, "*.yaml");
            }
            catch (Exception e)
            {
                log.Error(e.Message);
            }
            foreach (var file in files)
            {
                try
                {
                    var data = File.ReadAllText(file);
                    var tempalte = deserializer.Deserialize<Template>(data);
                    if (tempalte.IsTemplateValid())
                        ftuTemplates.Add(tempalte);
                    else
                    {
                        log.Info($"Template is not valid: {file}");
                        nonValidTemplates.Add(file);
                    }
                }
                catch (Exception e)
                {
                    log.Error($"Template is not valid: {e.Message}");
                    nonValidTemplates.Add(file);
                }
            }
            messages.Add($"Valid Templates: {ftuTemplates.Count + psfTemplates.Count}, Invalid Templates: {nonValidTemplates.Count}");
            foreach (var t in nonValidTemplates)
                messages.Add($"Template {t} is not valid");
        }

        public Template FindTemplate(List<Template> templates, string file)
        {
            foreach (var t in templates)
            {
                try
                {
                    var firstNonEmptyLines = File.ReadLines(file).Take(10).ToList();
                    foreach (var l in firstNonEmptyLines)
                    {
                        var regex = new Regex(t.MatchRegex);
                        if (regex.IsMatch(l))
                            return t;
                    }
                }
                catch (Exception e)
                {
                    log.Error(e.Message);
                }
            }
            return null;
        }

        private void GetLastOpenedDirectory(string file)
        {
            try
            {
                lastOpenedFolder = Path.GetDirectoryName(file);
            }
            catch (Exception e)
            {
                log.Error(e.Message);
                lastOpenedFolder = "";
            }
        }

        private async void ProcessFiles()
        {
            if (IsProcessFiles)
                CancelProcessing();
            else
            {
                var isTimeOffsetCorrect = int.TryParse(TimeOffset, out int timeOffset);
                if (!isTimeOffsetCorrect)
                {
                    await MessageBoxView.Show(App.App.CurrentWindow, "Time Offset is incorrect", "Error", MessageBoxView.MessageBoxButtons.Ok);
                    return;
                }

                UpdatingFileProgressBarValue = 0.00;
                IsProcessFiles = true;
                source = new CancellationTokenSource();
                fileToUpdateTotalLines = 0;
                foreach (var ftu in filesToUpdate)
                {
                    if (ftu.CoveringStatus != CoveringStatus.NotCovered)
                        fileToUpdateTotalLines += ftu.CalculateCountOfLines();
                }
                foreach (var ftu in filesToUpdate)
                {
                    messages.Add("Start Processing");
                    if (ftu.CoveringStatus != CoveringStatus.NotCovered)
                    {
                        fileToUpdateTotalLines = ftu.Coordinates.Count;
                        ftu.Parser.OnOneHundredLinesReplaced += UpdateProgressbar;
                        Interpolator.OnOneHundredLinesReplaced += UpdateProgressbar;
                        ftu.SegyParser.OnOneHundredLinesReplaced += UpdateProgressbar;
                        var message = await Task.Run(() => ftu.UpdateCoordinates(source, timeOffset));
                        Interpolator.OnOneHundredLinesReplaced -= UpdateProgressbar;
                        ftu.Parser.OnOneHundredLinesReplaced -= UpdateProgressbar;
                        ftu.SegyParser.OnOneHundredLinesReplaced -= UpdateProgressbar;
                        await MessageBoxView.Show(App.App.CurrentWindow, message, "Info", MessageBoxView.MessageBoxButtons.Ok);
                    }
                }
                IsProcessFiles = false;
            }
        }

        private void UpdateProgressbar(int lines)
        {
            UpdatingFileProgressBarValue = lines / (double)fileToUpdateTotalLines * 100;
        }

        public void CancelProcessing()
        {
            source.Cancel();
            source.Dispose();
            IsProcessFiles = false;
            UpdatingFileProgressBarValue = 0.00;
        }
    }
}