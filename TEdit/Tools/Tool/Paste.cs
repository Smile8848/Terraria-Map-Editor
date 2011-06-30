﻿using System;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TEdit.Common;
using TEdit.RenderWorld;
using TEdit.TerrariaWorld;
using TEdit.TerrariaWorld.Structures;
using TEdit.Tools.Clipboard;

namespace TEdit.Tools.Tool
{
    [Export(typeof(ITool))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    [ExportMetadata("Order", 2)]
    public class Paste : ToolBase, IPartImportsSatisfiedNotification
    {
        [Import]
        private ToolProperties _properties;

        [Import("World", typeof(World))]
        private World _world;

        public Paste()
        {
            _Image = new BitmapImage(new Uri(@"pack://application:,,,/TEdit;component/Images/Tools/paste.png"));
            _Name = "Paste";
            _Type = ToolType.Selection;
            _IsActive = false;
        }



        #region Properties

        private readonly BitmapImage _Image;
        private readonly string _Name;

        private readonly ToolType _Type;
        private bool _IsActive;

        [Import]
        private ClipboardManager _clipboardMan;

        [Import]
        private WorldRenderer _renderer;

        public override string Name
        {
            get { return _Name; }
        }

        public override ToolType Type
        {
            get { return _Type; }
        }

        public override BitmapImage Image
        {
            get { return _Image; }
        }

        public override bool IsActive
        {
            get { return _IsActive; }
            set
            {
                if (_IsActive != value)
                {
                    _IsActive = value;
                    RaisePropertyChanged("IsActive");
                }
                if (_IsActive)
                {
                    _properties.Mode = ToolAnchorMode.TopLeft;
                    UpdateSize();
                }
                else
                {
                    _properties.Mode = ToolAnchorMode.Center;
                }
            }
        }

        #endregion

        public override bool PressTool(TileMouseEventArgs e)
        {
            PasteClipboard(new PointInt32(e.Tile.X - _properties.Offset.X,
                                          e.Tile.Y - _properties.Offset.Y));
            return false;
        }


        public override bool MoveTool(TileMouseEventArgs e)
        {
            return false;
        }

        public override bool ReleaseTool(TileMouseEventArgs e)
        {
            // Do nothing on release
            return false;
        }

        public override WriteableBitmap PreviewTool()
        {
            return _renderer.RenderBuffer(_clipboardMan.Buffer);
        }

        private void PasteClipboard(PointInt32 anchor)
        {
            ClipboardBuffer.PasteBufferIntoWorld(_world, _clipboardMan.Buffer, anchor);
            _renderer.UpdateWorldImage(new Int32Rect(anchor.X, anchor.Y, _clipboardMan.Buffer.Size.X + 1, _clipboardMan.Buffer.Size.Y + 1));
        }

        private void ClipboardManPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Buffer")
            {
                if (_clipboardMan.Buffer != null)
                {
                    UpdateSize();
                }
            }
        }

        private void UpdateSize()
        {
            _properties.MinHeight = _clipboardMan.Buffer.Size.X;
            _properties.MinWidth = _clipboardMan.Buffer.Size.Y;
            _properties.MaxHeight = _clipboardMan.Buffer.Size.X;
            _properties.MaxWidth = _clipboardMan.Buffer.Size.Y;
        }

        public void OnImportsSatisfied()
        {
            _clipboardMan.PropertyChanged += ClipboardManPropertyChanged;
        }
    }
}