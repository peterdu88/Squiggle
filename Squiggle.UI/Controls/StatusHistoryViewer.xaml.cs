﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Squiggle.UI.Resources;
using Squiggle.Utilities;
using Squiggle.Chat;
using Squiggle.History;
using Squiggle.History.DAL;

namespace Squiggle.UI.Controls
{
    /// <summary>
    /// Interaction logic for StatusHistoryViewer.xaml
    /// </summary>
    public partial class StatusHistoryViewer : UserControl
    {
        Action lastSearch;

        public StatusHistoryViewer()
        {
            InitializeComponent();
        }

        void Search_Click(object sender, RoutedEventArgs e)
        {
            DateTime? from = txtFrom.SelectedDate;
            DateTime? to = txtTo.SelectedDate;
            lastSearch = () => AsyncInvoke(() => SearchUpdates(from, to));
            lastSearch();
        }

        void SearchUpdates(DateTime? from, DateTime? to)
        {
            var historyManager = new HistoryManager();
            var updates = historyManager.GetStatusUpdates(new StatusCriteria()
            {
                From = from,
                To = to,
            }).Select(update => new Result()
            {
                Time = update.Stamp,
                Name = update.ContactName,
                Status = (UserStatus)update.StatusCode
            }).ToList();

            Dispatcher.Invoke(() =>
            {
                results.ItemsSource = updates;
            });
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(Translation.Instance.HistoryViewer_ConfirmClear, "Squiggle", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                AsyncInvoke(() =>
                {
                    var historyManager = new HistoryManager();
                    historyManager.ClearStatusHistory();
                }, () => results.ItemsSource = null);
            }
        }

        void AsyncInvoke(Action action)
        {
            AsyncInvoke(action, () => { });
        }

        void AsyncInvoke(Action action, Action onComplete)
        {
            busyIndicator.IsBusy = true;
            Async.Invoke(() =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            },
            () =>
            {
                onComplete();
                busyIndicator.IsBusy = false;
            }
            , Dispatcher);
        }

        class Result
        {
            public DateTime Time { get; set; }
            public string Name { get; set; }
            public UserStatus Status { get; set; }
        }
    }
}
