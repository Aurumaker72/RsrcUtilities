using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using RsrcArchitect.ViewModels.Messages;
using RsrcCore.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RsrcArchitect.ViewModels
{
	public partial class ToolboxItemViewModel : ObservableObject
	{
		public ToolboxItemViewModel(string name, Func<Control> createControlFunction)
		{
			Name = name;
			_createControlFunction = createControlFunction;
		}

		public string Name { get; }
		private readonly Func<Control> _createControlFunction;

		[RelayCommand]
		private void Create()
		{
			WeakReferenceMessenger.Default.Send(new ControlAddingMessage(_createControlFunction()));
		}
	}
}
