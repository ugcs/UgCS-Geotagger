﻿using log4net;
using log4net.Config;
using log4net.Repository.Hierarchy;
using ReactiveUI;
using System.IO;
using System.Reflection;
using System.Xml;
using UgCSPPK.ViewModels;

namespace App.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private ViewModelBase _content;

        public ViewModelBase Content
        {
            get => _content;
            private set => this.RaiseAndSetIfChanged(ref _content, value);
        }

        public MainWindowViewModel()
        {
            Content = new PpkToolViewModel();
        }
    }
}