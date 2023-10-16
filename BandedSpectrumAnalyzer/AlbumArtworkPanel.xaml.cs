// Copyright (C) 2011, Jacob Johnston 
//
// Permission is hereby granted, free of charge, to any person obtaining a 
// copy of this software and associated documentation files (the "Software"), 
// to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, 
// and/or sell copies of the Software, and to permit persons to whom the 
// Software is furnished to do so, subject to the following conditions: 
//
// The above copyright notice and this permission notice shall be included in 
// all copies or substantial portions of the Software. 
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.  IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE. 

// CD Jewel Case Images Copyright LeMarquis
// Used under Creative Commons License.
// http://lemarquis.deviantart.com/art/JEWEL-CASE-Photoshop-file-69316052

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BandedSpectrumAnalyzer
{
    /// <summary>
    /// Interaction logic for AlbumArtworkPanel.xaml
    /// </summary>
    public partial class AlbumArtworkPanel : UserControl
    {
        #region Dependency Property Declarations
        public static readonly DependencyProperty AlbumArtImageProperty =
            DependencyProperty.Register("AlbumArtImage",
            typeof(ImageSource),
            typeof(AlbumArtworkPanel));
        #endregion

        #region Dependency Properties
        public ImageSource AlbumArtImage
        {
            get { return (ImageSource)GetValue(AlbumArtImageProperty); }
            set
            {
                SetValue(AlbumArtImageProperty, value);
            }
        }
        #endregion

        public AlbumArtworkPanel()
        {
            InitializeComponent();

            DependencyPropertyDescriptor backgroundDescriptor = DependencyPropertyDescriptor.FromProperty(AlbumArtImageProperty, typeof(AlbumArtworkPanel));
            backgroundDescriptor.AddValueChanged(this, AlbumArtChanged);
        }

        private void AlbumArtChanged(object sender, EventArgs e)
        {
            AlbumArtImageBox.Source = AlbumArtImage;
            if (AlbumArtImage == null)
            {
                AlbumOverlayImage.Visibility = System.Windows.Visibility.Hidden;
                NoAlbumArtImage.Visibility = System.Windows.Visibility.Visible;
                AlbumArtImageBox.Visibility = System.Windows.Visibility.Hidden;                
            }
            else
            {
                AlbumOverlayImage.Visibility = System.Windows.Visibility.Visible;
                NoAlbumArtImage.Visibility = System.Windows.Visibility.Hidden;
                AlbumArtImageBox.Visibility = System.Windows.Visibility.Visible;                
            }
        }
    }
}
