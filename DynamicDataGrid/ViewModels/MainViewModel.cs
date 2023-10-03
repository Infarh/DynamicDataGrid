﻿using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Markup;
using DynamicDataGrid.Services;
using MathCore.WPF.Commands;
using MathCore.WPF.ViewModels;
using Ookii.Dialogs.Wpf;

namespace DynamicDataGrid.ViewModels;

[MarkupExtensionReturnType(typeof(MainViewModel))]
public class MainViewModel() : TitledViewModel("Главное окно")
{
    private readonly IFileService _fileService = new FileService();

    private ObservableCollection<DataViewModel> _Files
        = new()
    {
        new("File 1", @"c:\data\data_file1.txt"),
        new("File 2", @"c:\data\d1\data_file2.txt"),
        new("File 3", @"c:\data\d2\d3\data_file3.txt"),
        new("File 4", @"c:\data\d3\d5\d7\d8\data_file4.txt"),
    }
        ;

    public ObservableCollection<DataViewModel> Files { get => _Files; set => Set(ref _Files!, value); }

    #region property SelectedFile : DataViewModel - Выбранный файл

    /// <Summary>Выбранный файл</Summary>
    private DataViewModel? _SelectedFile;

    /// <Summary>Выбранный файл</Summary>
    public DataViewModel? SelectedFile { get => _SelectedFile; set => Set(ref _SelectedFile, value); }

    #endregion

    private Command? _RemoveCommand;

    public ICommand RemoveCommand => _RemoveCommand ??= Command.New<DataViewModel>(v => _Files?.Remove(v!), v => _Files?.Contains(v!) ?? false);


    #region property NewFileName : string - Имя нового файла

    /// <Summary>Имя нового файла</Summary>
    private string _NewFileName = "New file 0";

    /// <Summary>Имя нового файла</Summary>
    public string NewFileName { get => _NewFileName; set => Set(ref _NewFileName!, value); }

    #endregion

    #region property NewFilePath : string - Путь нового файла

    /// <Summary>Путь нового файла</Summary>
    private string _NewFilePath = "c:\\123\\321.txt";

    /// <Summary>Путь нового файла</Summary>
    public string NewFilePath { get => _NewFilePath; set => Set(ref _NewFilePath!, value); }

    #endregion

    private Command? _AddCommand;

    public ICommand AddCommand => _AddCommand ??= Command.New(() => _Files?.Add(new(_NewFileName, _NewFilePath)));

    private Command? _FillGridCommand;

    public ICommand FillGridCommand => _FillGridCommand ??= Command.New(() => FillGrid());

    private void FillGrid()
    {
        _Files = new()
        {
        new("File 1", @"c:\data\data_file1.txt"),
        new("File 2", @"c:\data\d1\data_file2.txt"),
        new("File 3", @"c:\data\d2\d3\data_file3.txt"),
        new("File 4", @"c:\data\d3\d5\d7\d8\data_file4.txt"),
        };

        OnPropertyChanged(nameof(Files));

    }


    private Command? _ChangeRootFolderCommand;

    public ICommand ChangeRootFolderCommand => _ChangeRootFolderCommand ??= Command.New(() => ChangeRootFolder());

    private async void ChangeRootFolder()
    {
        var dialog = new VistaFolderBrowserDialog { Multiselect = false };
        if (!dialog.ShowDialog() == true)
            return;

        _Files = new ObservableCollection<DataViewModel>(await _fileService.CreateItemList(dialog.SelectedPath));

        OnPropertyChanged(nameof(Files));
    }


}
