﻿/*
  LittleBigMouse.Control.Core
  Copyright (c) 2021 Mathieu GRENET.  All right reserved.

  This file is part of LittleBigMouse.Control.Core.

    LittleBigMouse.Control.Core is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    LittleBigMouse.Control.Core is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with MouseControl.  If not, see <http://www.gnu.org/licenses/>.

	  mailto:mathieu@mgth.fr
	  http://www.mgth.fr
*/

using System;
using Avalonia.Controls;
using HLab.Icons.Annotations.Icons;
using HLab.Mvvm;
using HLab.Mvvm.Annotations;
using HLab.Mvvm.Avalonia;
using HLab.UserNotification;
using LittleBigMouse.DisplayLayout;
using LittleBigMouse.Plugins;
using LittleBigMouse.Zoning;

namespace LittleBigMouse.Ui.Avalonia.Main;

public class MainService : IMainService
{
    public IMonitorsLayout Layout { get; }

    readonly Func<IMainPluginsViewModel> _getMainViewModel;
    readonly IMvvmService _mvvmService;

    readonly IUserNotificationService _notify;
    readonly ILittleBigMouseClientService _littleBigMouseClientService;

    Window? _controlWindow = null;
    Action<IMainPluginsViewModel>? _actions;

    public MainService(
        Func<IMainPluginsViewModel> mainViewModelGetter,
        IMonitorsLayout layout,
        IMvvmService mvvmService,
        IIconService iconService,
        ILittleBigMouseClientService littleBigMouseClientService,
        IUserNotificationService notify
    )
    {
        _notify = notify;
        _littleBigMouseClientService = littleBigMouseClientService;

        _mvvmService = mvvmService;
        _getMainViewModel = mainViewModelGetter;

        _littleBigMouseClientService.StateChanged += _littleBigMouseClientService_StateChanged;

        Layout = layout;
    }


    public void ShowControl()
    {
        if (_controlWindow is { IsLoaded: true })
        {
            _controlWindow.Activate();
            return;
        }

        var viewModel = _getMainViewModel();
        viewModel.Layout = Layout;

        _actions?.Invoke(viewModel);

        _controlWindow = _mvvmService
            .MainContext
            .GetView<DefaultViewMode>(viewModel, typeof(IDefaultViewClass))
            ?.AsWindow();

        if (_controlWindow == null) return;

        _controlWindow.Closed += (s, a) => _controlWindow = null;
        _controlWindow?.Show();
    }

    public void StartNotifier()
    {
        _notify.Click += (s, a) => ShowControl();

        // TODO 
        //_notify.AddMenu(-1, "Check for update", CheckUpdate);
        _notify.AddMenu(-1, "Open","Icon/Start", ShowControl);
        _notify.AddMenu(-1, "Start","Icon/Start", () => _littleBigMouseClientService.Start(Layout.ComputeZones()));
        _notify.AddMenu(-1, "Stop","Icon/Stop", _littleBigMouseClientService.Stop);
        _notify.AddMenu(-1, "Exit", "Icon/Stop", Quit);

        _notify.SetIcon("icon/lbm_off",128);
        //_notify.SetIcon("icon/MonitorLocation",128);
        //_notify.SetIcon(Resources.lbm_off);
        _notify.Show();
    }

    public void AddControlPlugin(Action<IMainPluginsViewModel>? action)
    {
        _actions += action;
    }

    void Quit()
    {
        _littleBigMouseClientService.Quit();
        // TODO : Avalonia
        //Application.Current.Shutdown();
    }

    void _littleBigMouseClientService_StateChanged(object? sender, LittleBigMouseServiceEventArgs args)
    {
        switch (args.State)
        {
            case LittleBigMouseState.Running:
                _notify.SetIcon("icon/lbm_on",32);
                break;
            case LittleBigMouseState.Stopped:
            case LittleBigMouseState.Dead:
                _notify.SetIcon("icon/lbm_off",32);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(args.State), args.State, null);
        }
    }
}