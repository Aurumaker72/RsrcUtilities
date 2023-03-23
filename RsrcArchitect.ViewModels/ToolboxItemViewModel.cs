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
		public ToolboxItemViewModel(DialogEditorViewModel dialogEditorViewModel, string name, Func<Control> createControlFunction)
		{
			_dialogEditorViewModel = dialogEditorViewModel;
			_createControlFunction = createControlFunction;
			Name = name;
		}

		private readonly DialogEditorViewModel _dialogEditorViewModel;
		private readonly Func<Control> _createControlFunction;
		public string Name { get; }

		[RelayCommand]
		private void Create()
		{
			_dialogEditorViewModel.AddControl(_createControlFunction());
		}
	}
}
