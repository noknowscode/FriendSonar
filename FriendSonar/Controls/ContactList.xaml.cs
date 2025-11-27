using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using FriendSonar.Models;
using Windows.UI;

namespace FriendSonar.Controls;

public sealed partial class ContactList : UserControl
{
    private ObservableCollection<Friend> _allFriends = new();
    public ObservableCollection<Friend> Friends { get; } = new();

    private string _currentSortBy = "Distance";

    public ContactList()
    {
        this.InitializeComponent();
    }

    public void SetFriends(ObservableCollection<Friend> friends)
    {
        _allFriends.Clear();
        foreach (var friend in friends)
        {
            _allFriends.Add(friend);
        }

        ApplySort();
    }

    private void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is ComboBox comboBox && comboBox.SelectedItem is ComboBoxItem item)
        {
            _currentSortBy = item.Tag?.ToString() ?? "Distance";
            ApplySort();
        }
    }

    private void ApplySort()
    {
        Friends.Clear();

        // Sort
        var sorted = _currentSortBy switch
        {
            "Name" => _allFriends.OrderBy(f => f.Name),
            "Status" => _allFriends.OrderBy(f => f.Status).ThenBy(f => f.DistanceMilesValue),
            _ => _allFriends.OrderBy(f => f.DistanceMilesValue) // Default: Distance
        };

        foreach (var friend in sorted)
        {
            Friends.Add(friend);
        }
    }
}
