using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;

using DynamicDataGrid.ViewModels;

using Microsoft.Xaml.Behaviors;

namespace DynamicDataGrid.Behaviors;

public class DynamicColumnsBehavior : Behavior<DataGrid>
{
    private object? _LastItemSource;

    protected override void OnAttached()
    {
        base.OnAttached();

        var item_source_property = DependencyPropertyDescriptor
            .FromProperty(
                ItemsControl.ItemsSourceProperty, 
                AssociatedType  /* typeof(DataGrid) */);

        item_source_property.AddValueChanged(AssociatedObject, OnItemSourceChanged);

        AssociatedObject.SourceUpdated += OnSourceUpdated;
        AssociatedObject.AutoGenerateColumns = false;
        UpdateItemSource(AssociatedObject.ItemsSource);
    }

    private void OnItemSourceChanged(object? Sender, EventArgs E)
    {
        if(Sender is not DataGrid { ItemsSource: { } source}) return;
        UpdateItemSource(source);
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject.AutoGenerateColumns = true;
        AssociatedObject.SourceUpdated -= OnSourceUpdated;
    }

    private void OnSourceUpdated(object? sender, DataTransferEventArgs e) => UpdateItemSource(e.Source);

    private void UpdateItemSource(object NewItemSource)
    {
        if (_LastItemSource is INotifyCollectionChanged last_source)
            last_source.CollectionChanged -= OnSourceCollectionChanged;

        if (NewItemSource is INotifyCollectionChanged new_source)
            new_source.CollectionChanged += OnSourceCollectionChanged;
        
        _LastItemSource = NewItemSource;

        RegenerateColumns(NewItemSource as IEnumerable<DataViewModel>);
    }

    private void RegenerateColumns(IEnumerable<DataViewModel>? Items)
    {
        AssociatedObject.Columns.Clear();
        if(Items is null) return;

        AssociatedObject.Columns.Add(new DataGridTextColumn { Header = "Title", Binding = new Binding("Title") });
        AssociatedObject.Columns.Add(new DataGridTextColumn { Header = "Path", Binding = new Binding("FilePath") });

        var columns = Items.Max(v => v.PathComponents.Count());
        for (var i = 0; i < columns; i++)
            AssociatedObject.Columns.Add(new DataGridTextColumn { Header = $"Path[{i}]", Binding = new Binding($"PathComponents[{i}]") });
    }

    private void OnSourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                var added_item_columns_count = e.NewItems!.Cast<DataViewModel>().Single().PathComponents.Count();
                if(added_item_columns_count > AssociatedObject.Columns.Count - 2) 
                    for(var i = AssociatedObject.Columns.Count - 2; i < added_item_columns_count; i++)
                        AssociatedObject.Columns.Add(new DataGridTextColumn { Header = $"Path[{i}]", Binding = new Binding($"PathComponents[{i}]") });
                break;

            case NotifyCollectionChangedAction.Remove:
                var removed_item_columns_count = e.OldItems!.Cast<DataViewModel>().Single().PathComponents.Count();
                if (removed_item_columns_count == AssociatedObject.Columns.Count - 2)
                    goto case NotifyCollectionChangedAction.Reset;

                break;

            case NotifyCollectionChangedAction.Reset:
                RegenerateColumns(sender as IEnumerable<DataViewModel>);
                break;
        }
    }
}
