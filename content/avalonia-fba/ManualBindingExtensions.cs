using System.Collections;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace AvaloniaFba.Template;

public static class ThreadApartmentExtensions
{
    public static void SetApartmentStateAsSTA(this Thread thread)
    {
        if (!OperatingSystem.IsWindows())
            return;

        thread.SetApartmentState(ApartmentState.Unknown);
        thread.SetApartmentState(ApartmentState.STA);
    }
}

public static class ManualBindingExtensions
{
    public static TControl BindProperty<TControl, TViewModel, TValue>(
        this TControl control,
        AvaloniaProperty property,
        TViewModel viewModel,
        Expression<Func<TViewModel, TValue>> propertyExpression)
        where TControl : Control
        where TViewModel : INotifyPropertyChanged
    {
        ArgumentNullException.ThrowIfNull(control);
        ArgumentNullException.ThrowIfNull(property);
        ArgumentNullException.ThrowIfNull(viewModel);
        ArgumentNullException.ThrowIfNull(propertyExpression);

        var accessors = ExtractAccessors(propertyExpression);
        var propertyName = accessors.PropertyName;
        var getter = accessors.Getter;

        void ApplyValue()
        {
            control.SetValue(property, getter(viewModel));
        }

        ApplyValue();

        PropertyChangedEventHandler? handler = null;
        handler = (_, args) =>
        {
            if (string.IsNullOrEmpty(args.PropertyName) || args.PropertyName == propertyName)
            {
                ApplyValue();
            }
        };

        void OnDetached(object? sender, VisualTreeAttachmentEventArgs e)
        {
            viewModel.PropertyChanged -= handler;
            control.DetachedFromVisualTree -= OnDetached;
        }

        viewModel.PropertyChanged += handler;
        control.DetachedFromVisualTree += OnDetached;

        return control;
    }

    public static TControl BindContent<TControl, TViewModel, TValue>(
        this TControl control,
        TViewModel viewModel,
        Expression<Func<TViewModel, TValue>> propertyExpression)
        where TControl : ContentControl
        where TViewModel : INotifyPropertyChanged
        => control.BindProperty(ContentControl.ContentProperty, viewModel, propertyExpression);

    public static Button BindCommand<TViewModel>(
        this Button control,
        TViewModel viewModel,
        Expression<Func<TViewModel, ICommand>> commandExpression,
        Expression<Func<TViewModel, object?>>? parameterExpression = null)
        where TViewModel : INotifyPropertyChanged
    {
        control.BindProperty(Button.CommandProperty, viewModel, commandExpression);
        if (parameterExpression is not null)
        {
            control.BindProperty(Button.CommandParameterProperty, viewModel, parameterExpression);
        }
        return control;
    }

    public static TextBlock BindText<TViewModel>(
        this TextBlock control,
        TViewModel viewModel,
        Expression<Func<TViewModel, string>> propertyExpression)
        where TViewModel : INotifyPropertyChanged
        => control.BindProperty(TextBlock.TextProperty, viewModel, propertyExpression);

    public static TextBox BindTextTwoWay<TViewModel>(
        this TextBox control,
        TViewModel viewModel,
        Expression<Func<TViewModel, string>> propertyExpression)
        where TViewModel : INotifyPropertyChanged
    {
        ArgumentNullException.ThrowIfNull(control);
        ArgumentNullException.ThrowIfNull(viewModel);
        ArgumentNullException.ThrowIfNull(propertyExpression);

        var accessors = ExtractAccessors(propertyExpression);
        if (accessors.Setter is null)
        {
            throw new ArgumentException("Expression must reference a settable property.", nameof(propertyExpression));
        }

        control.BindProperty(TextBox.TextProperty, viewModel, propertyExpression);

        void UpdateViewModel() => accessors.Setter(viewModel, control.Text ?? string.Empty);

        void OnTextChanged(object? sender, TextChangedEventArgs e) => UpdateViewModel();

        void OnDetached(object? sender, VisualTreeAttachmentEventArgs e)
        {
            control.TextChanged -= OnTextChanged;
            control.DetachedFromVisualTree -= OnDetached;
        }

        control.TextChanged += OnTextChanged;
        control.DetachedFromVisualTree += OnDetached;

        return control;
    }

    public static TextBox BindText<TViewModel>(
        this TextBox control,
        TViewModel viewModel,
        Expression<Func<TViewModel, string>> propertyExpression)
        where TViewModel : INotifyPropertyChanged
        => control.BindProperty(TextBox.TextProperty, viewModel, propertyExpression);

    public static TToggle BindIsChecked<TToggle, TViewModel>(
        this TToggle control,
        TViewModel viewModel,
        Expression<Func<TViewModel, bool?>> propertyExpression)
        where TToggle : ToggleButton
        where TViewModel : INotifyPropertyChanged
        => control.BindProperty(ToggleButton.IsCheckedProperty, viewModel, propertyExpression);

    public static TToggle BindIsCheckedTwoWay<TToggle, TViewModel>(
        this TToggle control,
        TViewModel viewModel,
        Expression<Func<TViewModel, bool?>> propertyExpression)
        where TToggle : ToggleButton
        where TViewModel : INotifyPropertyChanged
    {
        ArgumentNullException.ThrowIfNull(control);
        ArgumentNullException.ThrowIfNull(viewModel);
        ArgumentNullException.ThrowIfNull(propertyExpression);

        var accessors = ExtractAccessors(propertyExpression);
        if (accessors.Setter is null)
        {
            throw new ArgumentException("Expression must reference a settable property.", nameof(propertyExpression));
        }

        control.BindProperty(ToggleButton.IsCheckedProperty, viewModel, propertyExpression);

        void OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property == ToggleButton.IsCheckedProperty)
            {
                var newValue = e.NewValue switch
                {
                    bool b => (bool?)b,
                    _ => null
                };

                accessors.Setter(viewModel, newValue);
            }
        }

        void OnDetached(object? sender, VisualTreeAttachmentEventArgs e)
        {
            control.PropertyChanged -= OnPropertyChanged;
            control.DetachedFromVisualTree -= OnDetached;
        }

        control.PropertyChanged += OnPropertyChanged;
        control.DetachedFromVisualTree += OnDetached;

        return control;
    }

    public static TRange BindValue<TRange, TViewModel>(
        this TRange control,
        TViewModel viewModel,
        Expression<Func<TViewModel, double>> propertyExpression)
        where TRange : RangeBase
        where TViewModel : INotifyPropertyChanged
        => control.BindProperty(RangeBase.ValueProperty, viewModel, propertyExpression);

    public static TRange BindValueTwoWay<TRange, TViewModel>(
        this TRange control,
        TViewModel viewModel,
        Expression<Func<TViewModel, double>> propertyExpression)
        where TRange : RangeBase
        where TViewModel : INotifyPropertyChanged
    {
        ArgumentNullException.ThrowIfNull(control);
        ArgumentNullException.ThrowIfNull(viewModel);
        ArgumentNullException.ThrowIfNull(propertyExpression);

        var accessors = ExtractAccessors(propertyExpression);
        if (accessors.Setter is null)
        {
            throw new ArgumentException("Expression must reference a settable property.", nameof(propertyExpression));
        }

        control.BindProperty(RangeBase.ValueProperty, viewModel, propertyExpression);

        void OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property == RangeBase.ValueProperty && e.NewValue is double value)
            {
                accessors.Setter(viewModel, value);
            }
        }

        void OnDetached(object? sender, VisualTreeAttachmentEventArgs e)
        {
            control.PropertyChanged -= OnPropertyChanged;
            control.DetachedFromVisualTree -= OnDetached;
        }

        control.PropertyChanged += OnPropertyChanged;
        control.DetachedFromVisualTree += OnDetached;

        return control;
    }

    public static TItemsControl BindItemsSource<TItemsControl, TViewModel>(
        this TItemsControl control,
        TViewModel viewModel,
        Expression<Func<TViewModel, IEnumerable>> propertyExpression)
        where TItemsControl : ItemsControl
        where TViewModel : INotifyPropertyChanged
        => control.BindProperty(ItemsControl.ItemsSourceProperty, viewModel, propertyExpression);

    public static SelectingItemsControl BindSelectedItem<TViewModel>(
        this SelectingItemsControl control,
        TViewModel viewModel,
        Expression<Func<TViewModel, object?>> propertyExpression)
        where TViewModel : INotifyPropertyChanged
        => control.BindProperty(SelectingItemsControl.SelectedItemProperty, viewModel, propertyExpression);

    public static SelectingItemsControl BindSelectedItemTwoWay<TViewModel>(
        this SelectingItemsControl control,
        TViewModel viewModel,
        Expression<Func<TViewModel, object?>> propertyExpression)
        where TViewModel : INotifyPropertyChanged
    {
        ArgumentNullException.ThrowIfNull(control);
        ArgumentNullException.ThrowIfNull(viewModel);
        ArgumentNullException.ThrowIfNull(propertyExpression);

        var accessors = ExtractAccessors(propertyExpression);
        if (accessors.Setter is null)
        {
            throw new ArgumentException("Expression must reference a settable property.", nameof(propertyExpression));
        }

        control.BindProperty(SelectingItemsControl.SelectedItemProperty, viewModel, propertyExpression);

        void OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property == SelectingItemsControl.SelectedItemProperty)
            {
                accessors.Setter(viewModel, e.NewValue);
            }
        }

        void OnDetached(object? sender, VisualTreeAttachmentEventArgs e)
        {
            control.PropertyChanged -= OnPropertyChanged;
            control.DetachedFromVisualTree -= OnDetached;
        }

        control.PropertyChanged += OnPropertyChanged;
        control.DetachedFromVisualTree += OnDetached;

        return control;
    }

    public static SelectingItemsControl BindSelectedIndex<TViewModel>(
        this SelectingItemsControl control,
        TViewModel viewModel,
        Expression<Func<TViewModel, int>> propertyExpression)
        where TViewModel : INotifyPropertyChanged
        => control.BindProperty(SelectingItemsControl.SelectedIndexProperty, viewModel, propertyExpression);

    public static SelectingItemsControl BindSelectedIndexTwoWay<TViewModel>(
        this SelectingItemsControl control,
        TViewModel viewModel,
        Expression<Func<TViewModel, int>> propertyExpression)
        where TViewModel : INotifyPropertyChanged
    {
        ArgumentNullException.ThrowIfNull(control);
        ArgumentNullException.ThrowIfNull(viewModel);
        ArgumentNullException.ThrowIfNull(propertyExpression);

        var accessors = ExtractAccessors(propertyExpression);
        if (accessors.Setter is null)
        {
            throw new ArgumentException("Expression must reference a settable property.", nameof(propertyExpression));
        }

        control.BindProperty(SelectingItemsControl.SelectedIndexProperty, viewModel, propertyExpression);

        void OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property == SelectingItemsControl.SelectedIndexProperty && e.NewValue is int index)
            {
                accessors.Setter(viewModel, index);
            }
        }

        void OnDetached(object? sender, VisualTreeAttachmentEventArgs e)
        {
            control.PropertyChanged -= OnPropertyChanged;
            control.DetachedFromVisualTree -= OnDetached;
        }

        control.PropertyChanged += OnPropertyChanged;
        control.DetachedFromVisualTree += OnDetached;

        return control;
    }

    private static PropertyAccessors<TViewModel, TValue> ExtractAccessors<TViewModel, TValue>(Expression<Func<TViewModel, TValue>> expression)
    {
        Expression body = expression.Body;

        if (body is UnaryExpression { NodeType: ExpressionType.Convert or ExpressionType.ConvertChecked, Operand: var operand })
        {
            body = operand;
        }

        if (body is not MemberExpression memberExpression)
        {
            throw new ArgumentException("Expression must reference a property.", nameof(expression));
        }

        if (memberExpression.Member is not PropertyInfo propertyInfo)
        {
            throw new ArgumentException("Expression must reference a property.", nameof(expression));
        }

        if (memberExpression.Expression is not ParameterExpression)
        {
            throw new ArgumentException("Expression must directly reference a property on the view model.", nameof(expression));
        }

        var getter = expression.Compile();
        Action<TViewModel, TValue>? setter = null;

        if (propertyInfo.SetMethod is { IsPublic: true })
        {
            var target = Expression.Parameter(typeof(TViewModel), "target");
            var value = Expression.Parameter(typeof(TValue), "value");
            Expression property = Expression.Property(target, propertyInfo);
            Expression valueExpression = property.Type == typeof(TValue)
                ? value
                : Expression.Convert(value, property.Type);
            var assign = Expression.Assign(property, valueExpression);
            setter = Expression.Lambda<Action<TViewModel, TValue>>(assign, target, value).Compile();
        }

        return new PropertyAccessors<TViewModel, TValue>(propertyInfo.Name, getter, setter);
    }

    private sealed record PropertyAccessors<TViewModel, TValue>(string PropertyName, Func<TViewModel, TValue> Getter, Action<TViewModel, TValue>? Setter);
}
