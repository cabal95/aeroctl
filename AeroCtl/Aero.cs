﻿using NativeWifi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AeroCtl
{
	/// <summary>
	/// Implements the AERO interfaces.
	/// </summary>
	public class Aero : IDisposable
	{
		#region Fields

		private WlanClient wlanClient;

		#endregion

		#region Properties

		/// <summary>
		/// Gets the WMI interface.
		/// </summary>
		private AeroWmi Wmi { get; }

		public string BaseBoard => this.Wmi.BaseBoard;

		/// <summary>
		/// Gets Keyboard Fn key handler.
		/// </summary>
		public KeyHandler Keys { get; }

		/// <summary>
		/// Gets the fan controller.
		/// </summary>
		public FanController Fans { get; }

		/// <summary>
		/// Gets the screen controller.
		/// </summary>
		public ScreenController Screen { get; }

		/// <summary>
		/// Gets the CPU temperature as reported by the Gigabyte ACPI interface.
		/// </summary>
		public int CpuTemperature => this.Wmi.InvokeGet<ushort>("getCpuTemp");

		/// <summary>
		/// Gets or sets the software wifi enable state.
		/// </summary>
		public bool WifiEnabled
		{
			get
			{
				Wlan.Dot11RadioState state = this.wlanClient.Interfaces.FirstOrDefault()?.RadioState.PhyRadioState.FirstOrDefault().dot11SoftwareRadioState ?? Wlan.Dot11RadioState.Unknown;
				return state == Wlan.Dot11RadioState.On;
			}
			set
			{
				Wlan.WlanPhyRadioState newState;
				if (value)
				{
					newState = new Wlan.WlanPhyRadioState
					{
						dwPhyIndex = (int)Wlan.Dot11PhyType.Any,
						dot11SoftwareRadioState = Wlan.Dot11RadioState.On,
					};
				}
				else
				{
					newState = new Wlan.WlanPhyRadioState
					{
						dwPhyIndex = (int)Wlan.Dot11PhyType.Any,
						dot11SoftwareRadioState = Wlan.Dot11RadioState.Off,
					};
				}

				foreach (var iface in this.wlanClient.Interfaces)
				{
					iface.SetRadioState(newState);
				}
			}
		}

		#endregion

		#region Constructors

		public Aero(AeroWmi wmi)
		{
			this.Wmi = wmi;
			this.Keys = new KeyHandler();
			this.Fans = new FanController(wmi);
			this.Screen = new ScreenController(wmi);
			this.wlanClient = new WlanClient();
		}

		#endregion

		#region Methods

		public void Dispose()
		{
			this.Wmi?.Dispose();
			this.Keys?.Dispose();
		}

		#endregion
	}
}
