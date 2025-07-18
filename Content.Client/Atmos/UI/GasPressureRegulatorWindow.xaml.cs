﻿using System.Globalization;
using Content.Client.UserInterface.Controls;
using Robust.Client.AutoGenerated;
using Robust.Client.UserInterface.XAML;
using Robust.Shared.Timing;

namespace Content.Client.Atmos.UI;

/// <summary>
/// Client-side UI for controlling a pressure regulator.
/// </summary>
[GenerateTypedNameReferences]
public sealed partial class GasPressureRegulatorWindow : FancyWindow
{
    private float _flowRate;

    public GasPressureRegulatorWindow()
    {
        RobustXamlLoader.Load(this);

        ThresholdInput.OnTextChanged += _ => SetThresholdButton.Disabled = false;
        SetThresholdButton.OnPressed += _ =>
        {
            ThresholdPressureChanged?.Invoke(ThresholdInput.Text ??= "");
            SetThresholdButton.Disabled = true;
        };

        SetToCurrentPressureButton.OnPressed += _ =>
        {
            if (InletPressureLabel.Text != null)
            {
                ThresholdInput.Text = InletPressureLabel.Text;
            }

            SetThresholdButton.Disabled = false;
        };

        ZeroThresholdButton.OnPressed += _ =>
        {
            ThresholdInput.Text = "0";
            SetThresholdButton.Disabled = false;
        };

        Add1000Button.OnPressed += _ => AdjustThreshold(1000);
        Add100Button.OnPressed += _ => AdjustThreshold(100);
        Add10Button.OnPressed += _ => AdjustThreshold(10);
        Subtract10Button.OnPressed += _ => AdjustThreshold(-10);
        Subtract100Button.OnPressed += _ => AdjustThreshold(-100);
        Subtract1000Button.OnPressed += _ => AdjustThreshold(-1000);
        return;

        void AdjustThreshold(float adjustment)
        {
            if (float.TryParse(ThresholdInput.Text, out var currentValue))
            {
                ThresholdInput.Text = (currentValue + adjustment).ToString(CultureInfo.CurrentCulture);
                SetThresholdButton.Disabled = false;
            }
        }
    }

    public event Action<string>? ThresholdPressureChanged;

    /// <summary>
    /// Sets the current threshold pressure label. This is not setting the threshold input box.
    /// </summary>
    /// <param name="threshold"> Threshold to set.</param>
    public void SetThresholdPressureLabel(float threshold)
    {
        TargetPressureLabel.Text = threshold.ToString(CultureInfo.CurrentCulture);
    }

    /// <summary>
    /// Sets the threshold pressure input field with the given value.
    /// When the client opens the UI the field will be autofilled with the current threshold pressure.
    /// </summary>
    /// <param name="input">The threshold pressure value to autofill into the input field.</param>
    public void SetThresholdPressureInput(float input)
    {
        ThresholdInput.Text = input.ToString(CultureInfo.CurrentCulture);
    }

    /// <summary>
    /// Sets the entity to be visible in the UI.
    /// </summary>
    /// <param name="entity"></param>
    public void SetEntity(EntityUid entity)
    {
        EntityView.SetEntity(entity);
    }

    /// <summary>
    /// Updates the UI for the labels.
    /// </summary>
    /// <param name="inletPressure">The current pressure at the valve's inlet.</param>
    /// <param name="outletPressure">The current pressure at the valve's outlet.</param>
    /// <param name="flowRate">The current flow rate through the valve.</param>
    public void UpdateInfo(float inletPressure, float outletPressure, float flowRate)
    {
        if (float.TryParse(TargetPressureLabel.Text, out var parsedfloat))
            ToTargetBar.Value = inletPressure / parsedfloat;

        InletPressureLabel.Text = float.Round(inletPressure).ToString(CultureInfo.CurrentCulture);
        OutletPressureLabel.Text = float.Round(outletPressure).ToString(CultureInfo.CurrentCulture);
        CurrentFlowLabel.Text = float.IsNaN(flowRate) ? "0" : flowRate.ToString(CultureInfo.CurrentCulture);
        _flowRate = flowRate;
    }

    protected override void FrameUpdate(FrameEventArgs args)
    {
        base.FrameUpdate(args);

        // Defines the flow rate at which the progress bar fills in one second.
        // If the flow rate is >50 L/s, the bar will take <1 second to fill.
        // If the flow rate is <50 L/s, the bar will take >1 second to fill.
        const int barFillPerSecond = 50;

        var maxValue = FlowRateBar.MaxValue;

        // Increment the progress bar value based on elapsed time
        FlowRateBar.Value += (_flowRate / barFillPerSecond) * args.DeltaSeconds;

        // Reset the progress bar when it is fully filled
        if (FlowRateBar.Value >= maxValue)
        {
            FlowRateBar.Value = 0f;
        }
    }
}
