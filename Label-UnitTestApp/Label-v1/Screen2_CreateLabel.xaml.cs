using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Input;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using ZXing;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Label_v1
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Screen2_CreateLabel : Page
    {
        //Variables
        Label label = new Label(), tempLabel;
        int selectedIndex;
        bool validState = false, textMovedValid=false, imageMovedValid=false, barcodeMovedValid=false, printUnregistered=true;
        string[] allFonts = Microsoft.Graphics.Canvas.Text.CanvasTextFormat.GetSystemFontFamilies();
        UIElement movableElement;
        PointerPoint ptr;
        PrintHelper printHelper;
        Database database;
        //Main Function
        public Screen2_CreateLabel()
        {
        //    this.InitializeComponent();
         //   ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.FullScreen;
            foreach (string x in allFonts)
            {
                ComboboxFonts.Items.Add(x);
            }
            validState = true;
            label.previewLabelGridWidth = PreviewLabelGrid.Width;
            label.previewLabelGridHeight = PreviewLabelGrid.Height;

            for (int i = 2; i < 201; i++)
            {
                numOfLabels.Items.Add(i);
            }
            label.numOfPage = numOfLabels.SelectedIndex + 1;
        }
        public enum Mode
        {
            AlphaNumeric = 1,
            Alpha = 2,
            Numeric = 3
        }
        public string Increment(string text,Mode mode)
        {
            var textArr = text.ToCharArray();

            // Add legal characters
            var characters = new List<char>();
            if (mode == Mode.AlphaNumeric || mode == Mode.Numeric)
                for (char c = '0'; c <= '9'; c++)
                    characters.Add(c);
            if (mode == Mode.AlphaNumeric || mode == Mode.Alpha)
                for (char c = 'a'; c <= 'z'; c++)
                    characters.Add(c);

            bool check = false;
            // Loop from end to beginning
            for (int i = textArr.Length - 1; i >= 0; i--)
            {
                if (textArr[i] == 'z' || textArr[i] == '9')
                {
                   foreach(char x in textArr)
                    {
                        if (x == 'z' || x == '9') check = true;
                        else
                        {
                            check = false;
                            break;
                        }
                    }
                    if (check)
                    {
                        if (mode == Mode.AlphaNumeric)
                        {
                            if (char.IsLetter(textArr[0]))
                            {
                                textArr = textArr.Concat(new[] { '0' }).ToArray();
                                for (int z = textArr.Length - 1; z >= 0; z--)
                                {
                                    if (char.IsLetter(textArr[z]))
                                    {
                                        textArr[z] = characters[characters.IndexOf('a')];
                                        textArr[z + 1] = textArr[z];
                                    }
                                        if (char.IsDigit(textArr[z])) textArr[z] = characters[characters.IndexOf('0')];
                                }
                                break;
                            }
                            else
                            {
                                ErrorText.Visibility = Visibility.Visible;
                                ErrorText.Text = "Barcode key should start with an alphabet if it is an AplhaNemeric series.";
                                return "0";
                            }
                        }
                        else if(mode == Mode.Numeric) 
                        {
                            textArr = textArr.Concat(new[] { '0' }).ToArray();
                            for(int z = textArr.Length - 2; z > 0; z--)
                            {
                                textArr[z] = characters[characters.IndexOf('0')];
                            }
                            textArr[0] = characters[characters.IndexOf('1')];
                            break;
                        }
                    else if (mode == Mode.Alpha)
                        {
                            textArr = textArr.Concat(new[] { 'a' }).ToArray();
                            for (int z = textArr.Length - 2; z > 0; z--)
                            {
                                textArr[z] = characters[characters.IndexOf('a')];
                            }
                            textArr[0] = characters[characters.IndexOf('a')];
                            break;
                        }
                    }
                    else
                    {
                        if (mode == Mode.AlphaNumeric)
                        {
                            if (char.IsLetter(textArr[0]))
                            {
                                if (char.IsLetter(textArr[i])) textArr[i] = characters[characters.IndexOf('a')];
                                else if (char.IsDigit(textArr[i])) textArr[i] = characters[characters.IndexOf('0')];
                            }
                            else
                            {
                                ErrorText.Visibility = Visibility.Visible;
                                ErrorText.Text = "Barcode key should start with an alphabet if it is an AplhaNemeric series.";
                                return "0";
                            }
                        }
                        else if (mode == Mode.Numeric) textArr[i] = characters[characters.IndexOf('0')];
                        else if (mode == Mode.Alpha) textArr[i] = characters[characters.IndexOf('a')];
                    }
                }
                else
                {
                    textArr[i] = characters[characters.IndexOf(textArr[i]) + 1];
                    break;
                }
            }

            return new string(textArr);
        }

        //Defualt Functions
        private void RowValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (!validState) return;
            try
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                label.ArrRowColumn[0] = (long)RowSlider.Value;
                if(label.barcodeValid) funcBarcodeUpdate();
                funcRegisterForPrinting();
            }
            catch
            {

            }
        }

        private void ColumnValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (!validState) return;
            try
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                label.ArrRowColumn[1] = (long)ColumnSlider.Value;
                if(label.barcodeValid)  funcBarcodeUpdate();
                funcRegisterForPrinting();
            }
            catch
            {

            }
        }

        private void AddButtonClicked(object sender, RoutedEventArgs e)
        {
            if (!validState) return;
            try
            {
                funcCreateTextbox();
                ++label.varAddButton;
                if (label.varAddButton > 0)
                {
                    SubButton.IsEnabled = true;
                }
                funcCreateLabel();
                funcRegisterForPrinting();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            catch
            {

            }
        }

        private void SubButtonClicked(object sender, RoutedEventArgs e)
        {
            if (!validState) return;
            try
            {
                funcRemoveTextbox();
                label.varAddButton--;
                if (label.varAddButton == 0)
                {
                    SubButton.IsEnabled = false;
                }
                 funcCreateLabel();
                funcRegisterForPrinting();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            catch
            {

            }
        }

        private void BackButtonClicked(object sender, RoutedEventArgs e)
        {
            if (!validState) return;
            if (Frame.CanGoBack) {

                Frame.GoBack();
            }
        }

        private void LabelNameTextBoxChanged(object sender, TextChangedEventArgs e)
        {
            if (!validState) return;
            try
            {
                label.labelName = Textbox0.Text;
                funcRegisterForPrinting();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            catch
            {

            }
        }
        
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

            tempLabel = new Label(label);
            printHelper = new PrintHelper(this);
            printHelper.RegisterForPrinting(tempLabel);

            printHelper.PreparePrintContent(new PageToPrint());
            printUnregistered = false;

            database = e.Parameter as Database;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (printHelper != null)
            {
                printHelper.UnRegisterForPrinting();
            }
            printUnregistered = true;
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
        
        //TextBlock functions

        private void SliderFontSizeChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (!validState) return;
            try
            {
                (PreviewLabelGrid.Children.ElementAt(PreviewLabelGrid.Children.IndexOf(movableElement)) as TextBlock).FontSize = SliderFontSize.Value;
                label.ArrTextFont[selectedIndex] = SliderFontSize.Value;
            }
            catch
            {

            }
        }

        private void ComboboxFontsChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!validState) return;
            try
            {
                (PreviewLabelGrid.Children.ElementAt(PreviewLabelGrid.Children.IndexOf(movableElement)) as TextBlock).FontFamily = new FontFamily(ComboboxFonts.SelectedItem.ToString());
                label.ArrTextFontFamily[selectedIndex] = ComboboxFonts.SelectedItem.ToString();
            }
            catch
            {

            }
           
        }

        private void MoreSettingsOkClicked(object sender, RoutedEventArgs e)
        {
            if (!validState) return;
            CreateLabelSettingsScrollViewer.Visibility = Visibility.Visible;
            MoreSettingsScrollView.Visibility = Visibility.Collapsed;
           
            SliderFontSize.IsEnabled = false;
            ComboboxFonts.IsEnabled = false;

            StatusTextBlock.Text = "Select a Text Block";
            StackStatusTextBlock.BorderThickness = new Thickness(0);
            StackStatusTextBlock.BorderBrush = new SolidColorBrush(Colors.Transparent);
            funcRegisterForPrinting();
            GC.Collect();
        }

        //Custom Functions

        void funcRegisterForPrinting()
        {
            if (!printUnregistered)
            {
                if (printHelper != null)
                {
                    printHelper.UnRegisterForPrinting();
                }
                printUnregistered = true;
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            if (printUnregistered)
            {
                tempLabel = new Label(label);
                printHelper = new PrintHelper(this);
                printHelper.RegisterForPrinting(tempLabel);

                printHelper.PreparePrintContent(new PageToPrint());
                printUnregistered = false;
            }
        }

        void funcClearLabel()
        {
            if (!validState) return;
            PreviewLabelGrid.Children.Clear();
        }

        void funcCreateTextbox()
        {
            if (!validState) return;
            var textbox = new TextBox();
                textbox.Name = label.varAddButton.ToString();
                textbox.TextChanged += Textbox_TextChanged;
                textbox.PlaceholderText = "Enter Text Line " + (label.varAddButton + 1);
                textbox.Margin = new Thickness(10);
                textbox.Foreground = new SolidColorBrush(Colors.Black);
                textbox.TextAlignment = TextAlignment.Left;
                textbox.FontSize = 15;
                textbox.FontFamily = new FontFamily("Arial");
                textbox.Background = new SolidColorBrush(Colors.White);
            
            var button = new Button();
            button.Name =  label.varAddButton.ToString();
            button.FontFamily = new FontFamily("Segoe MDL2 Assets");
            button.Content = char.ConvertFromUtf32(0xE106);
            button.Margin = new Thickness(0,10,10,10);
            button.Click += Button_Click;
            button.Width = 32;
            button.Height = 32;
            button.Style = ButtonsSettings;

            int rnd = RandomGenerator(0);
            int check = 0;
            foreach (double x in label.ArrTextLeftMargin)
            {
                while (check == 0)
                {
                    if ((x + 20) <= rnd || (x - 20) >= rnd) break;
                    else rnd = RandomGenerator(0);
                }
            }
            label.ArrTextLeftMargin.Insert((int)label.varAddButton, rnd);
            rnd = RandomGenerator(1);
            foreach (double x in label.ArrTextTopMargin)
            {
                while (check == 0)
                {
                    if((x + 20) <= rnd || (x - 20) >= rnd) break;
                    else rnd = RandomGenerator(1);
                }
            }
            label.ArrTextTopMargin.Insert((int)label.varAddButton, rnd);
            label.ArrTextFont.Insert((int)label.varAddButton, 15);
            label.ArrTextFontFamily.Insert((int)label.varAddButton,"Arial");
            label.ArrText.Insert((int)label.varAddButton, "Text Line " + (label.varAddButton + 1));
            label.ArrXAlign.Insert((int)label.varAddButton, 0);
            label.ArrYAlign.Insert((int)label.varAddButton, 0);
            StackButton.Children.Add(button);

            StackTextBox.Children.Add(textbox);
        }

        private int RandomGenerator(int x)
        {
            Random rnd = new Random();
            if(x == 0)
            {
                int leftMargin = rnd.Next((int)PreviewLabelGrid.ActualWidth/2 - 1);
                return leftMargin;
            }
            else if(x == 1)
            {
                int topMargin = rnd.Next((int)PreviewLabelGrid.ActualHeight - 1);
                return topMargin;
            }
            return 0;         
        }
          
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!validState) return;
            var button = sender as Button;
            int num = Convert.ToInt32(button.Name);
            StackButton.Children.RemoveAt(num);
            StackTextBox.Children.RemoveAt(num);

            label.ArrText.RemoveAt(num);
            label.ArrTextLeftMargin.RemoveAt(num);
            label.ArrTextTopMargin.RemoveAt(num);
            label.ArrTextFontFamily.RemoveAt(num);
            label.ArrTextFont.RemoveAt(num);
            label.ArrXAlign.RemoveAt(num);
            label.ArrYAlign.RemoveAt(num);

            label.varAddButton--;

            for (int i = 0; i< StackTextBox.Children.Count; i++)
            {
                TextBox textbox = StackTextBox.Children.ElementAt(i) as TextBox;
                textbox.Name = i.ToString();
                textbox.PlaceholderText = "Enter Text Line " + (i+1);
                if(label.ArrText[i].Contains("Text Line ")){
                    label.ArrText[i] = "Text Line " + (i + 1);
                }

                Button button1 = StackButton.Children.ElementAt(i) as Button;
                button1.Name = i.ToString();

            }
            funcCreateLabel();
            funcRegisterForPrinting();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        void funcRemoveTextbox()
        {
            if (!validState) return;
            StackTextBox.Children.RemoveAt((int)label.varAddButton - 1);
            StackButton.Children.RemoveAt((int)label.varAddButton - 1);
        }

        private void Textbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!validState) return;
            try
            {
                var textbox = (sender as TextBox);
                label.ArrText[Convert.ToInt32(textbox.Name)] = textbox.Text;
                funcCreateLabel();
                funcRegisterForPrinting();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            catch
            {

            }
        }
        
        void funcCreateLabel() {
            if (!validState) return;
            funcClearLabel();
            for(int i=0; i<label.varAddButton; i++)
            {
                var textblock = new TextBlock();
                textblock.Name = i.ToString();
                textblock.FontSize = label.ArrTextFont[i];
                textblock.FontFamily = new FontFamily(label.ArrTextFontFamily[i]);
                textblock.Foreground = new SolidColorBrush(Colors.Black);
                textblock.Text = label.ArrText[i];
                textblock.Tapped += Textblock_Tapped;
                textblock.PointerPressed += Textblock_PointerPressed;
                textblock.Margin = new Thickness(label.ArrTextLeftMargin[i], label.ArrTextTopMargin[i],0,0);

                if(label.ArrXAlign[i] == 0) textblock.HorizontalAlignment = HorizontalAlignment.Left;
                else if(label.ArrXAlign[i] == 1) textblock.HorizontalAlignment = HorizontalAlignment.Center;
                else if (label.ArrXAlign[i] == 2) textblock.HorizontalAlignment = HorizontalAlignment.Right;

                if(label.ArrYAlign[i] == 0) textblock.VerticalAlignment = VerticalAlignment.Top;
                else if (label.ArrYAlign[i] == 1) textblock.VerticalAlignment = VerticalAlignment.Center;
                else if (label.ArrYAlign[i] == 2) textblock.VerticalAlignment = VerticalAlignment.Bottom;
                
                PreviewLabelGrid.Children.Add(textblock);

            }
            if (label.varImageCount != 0)
            {

                for (int i = 0; i < label.varImageCount; i++)
                {
                    var image = new Image();
                    image.Source = label.ImageBitmap[i];
                    image.Name = label.ImageFileName[i];
                    image.Width = label.ImageWidth[i];
                    image.Height = label.ImageHeight[i];
                    image.Margin = new Thickness(label.ImageLeftMargin[i], label.ImageTopMargin[i], 0, 0);
                    image.Tapped += Image_Tapped;
                    image.HorizontalAlignment = HorizontalAlignment.Left;
                    image.PointerPressed += Image_PointerPressed;
                    image.VerticalAlignment = VerticalAlignment.Top;
                    movableElement = image as UIElement;
                    if (label.ImageStretchAlignment[i] == 0) image.Stretch = Stretch.None;
                    else if (label.ImageStretchAlignment[i] == 1) image.Stretch = Stretch.Fill;
                    else if (label.ImageStretchAlignment[i] == 2) image.Stretch = Stretch.Uniform;
                    else if (label.ImageStretchAlignment[i] == 3) image.Stretch = Stretch.UniformToFill;

                    PreviewLabelGrid.Children.Add(image);
                }
            }
            if (label.barcodeValid)
            {
                var barcode = new Image();
                barcode.Name = "BarcodeImage";
                barcode.Width = label.barWidth;
                barcode.Height = label.barHeight;
                barcode.Margin = new Thickness(label.barLeftMargin, label.barTopMargin, 0, 0);
                barcode.Tapped += Barcode_tapped;
                barcode.Stretch = Stretch.Fill;
                barcode.PointerPressed += Barcode_PointerPressed;
                barcode.VerticalAlignment = VerticalAlignment.Top;
                barcode.HorizontalAlignment = HorizontalAlignment.Left;
                movableElement = barcode as UIElement;

                PreviewLabelGrid.Children.Add(barcode);
                funcBarcodeUpdate();
                barcode.Source = label.barKeyBitmap[0];
            }

        }
        
        private void Textblock_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (!validState) return;
            try
            {
                validState = false;

                CreateLabelSettingsScrollViewer.Visibility = Visibility.Collapsed;
                ImageSettings.Visibility = Visibility.Collapsed;
                BarcodeSettings.Visibility = Visibility.Collapsed;
                MoreSettingsScrollView.Visibility = Visibility.Visible;

                var textblock = sender as TextBlock;
                selectedIndex = Convert.ToInt32(textblock.Name);
                movableElement = sender as UIElement;
                StatusTextBlock.Text = textblock.Text;
                StackStatusTextBlock.BorderThickness = new Thickness(2);
                StackStatusTextBlock.BorderBrush = new SolidColorBrush(Colors.Wheat);

                SliderFontSize.IsEnabled = true;
                ComboboxFonts.IsEnabled = true;

                if (label.ArrXAlign[selectedIndex] == 0) XAlignmentCombobox.SelectedIndex = 0;
                else if (label.ArrXAlign[selectedIndex] == 1) XAlignmentCombobox.SelectedIndex = 1;
                else if (label.ArrXAlign[selectedIndex] == 2) XAlignmentCombobox.SelectedIndex = 2;

                if (label.ArrYAlign[selectedIndex] == 0) YAlignmentCombobox.SelectedIndex = 0;
                else if (label.ArrYAlign[selectedIndex] == 1) YAlignmentCombobox.SelectedIndex = 1;
                else if (label.ArrYAlign[selectedIndex] == 2) YAlignmentCombobox.SelectedIndex = 2;
                ComboboxFonts.SelectedIndex = ComboboxFonts.Items.IndexOf(label.ArrTextFontFamily[selectedIndex]);
                SliderFontSize.Value = label.ArrTextFont[selectedIndex];

                validState = true;
            }
            catch
            {

            }

        }

        private void funcBarcodeUpdate()
        {
            try
            {
                if (!validState || !label.barcodeValid) return;
                ErrorText.Visibility = Visibility.Collapsed;
                label.barKeyBitmap.Clear();
                BarcodeFormat tempFormat = new BarcodeFormat();

                if (label.bartype == "AZTEC") tempFormat = BarcodeFormat.AZTEC;
                else if (label.bartype == "CODABAR") tempFormat = BarcodeFormat.CODABAR;
                else if (label.bartype == "CODE_128") tempFormat = BarcodeFormat.CODE_128;
                else if (label.bartype == "CODE_39") tempFormat = BarcodeFormat.CODE_39;
                else if (label.bartype == "CODE_93") tempFormat = BarcodeFormat.CODE_93;
                else if (label.bartype == "DATA_MATRIX") tempFormat = BarcodeFormat.DATA_MATRIX;
                else if (label.bartype == "EAN_13") tempFormat = BarcodeFormat.EAN_13;
                else if (label.bartype == "EAN_8") tempFormat = BarcodeFormat.EAN_8;
                else if (label.bartype == "ITF") tempFormat = BarcodeFormat.ITF;
                else if (label.bartype == "MAXICODE") tempFormat = BarcodeFormat.MAXICODE;
                else if (label.bartype == "MSI") tempFormat = BarcodeFormat.MSI;
                else if (label.bartype == "PDF_417") tempFormat = BarcodeFormat.PDF_417;
                else if (label.bartype == "PLESSEY") tempFormat = BarcodeFormat.PLESSEY;
                else if (label.bartype == "QR_CODE") tempFormat = BarcodeFormat.QR_CODE;
                else if (label.bartype == "RSS_14") tempFormat = BarcodeFormat.RSS_14;
                else if (label.bartype == "RSS_EXPANDED") tempFormat = BarcodeFormat.RSS_EXPANDED;
                else if (label.bartype == "UPC_A") tempFormat = BarcodeFormat.UPC_A;
                else if (label.bartype == "UPC_E") tempFormat = BarcodeFormat.UPC_E;
                else if (label.bartype == "UPC_EAN_EXTENSION") tempFormat = BarcodeFormat.UPC_EAN_EXTENSION;

                IBarcodeWriter writer = new BarcodeWriter
                {
                    Format = tempFormat,
                    Options = new ZXing.Common.EncodingOptions
                    {
                        Height = (int)label.barHeight,
                        Width = (int)label.barWidth
                    },
                    Renderer = new ZXing.Rendering.PixelDataRenderer() { Foreground = Colors.Black }
                };
                if (!label.barMode)
                {
                    var result = writer.Write(label.barKey);
                    label.barKeyBitmap.Add(result.ToBitmap() as WriteableBitmap);
                    (PreviewLabelGrid.Children.ElementAt(PreviewLabelGrid.Children.IndexOf(movableElement)) as Image).Source = label.barKeyBitmap[0];
                }
                else if (label.barMode)
                {
                    var result = writer.Write(label.barKey);
                    label.barKeyBitmap.Add(result.ToBitmap() as WriteableBitmap);
                    (PreviewLabelGrid.Children.ElementAt(PreviewLabelGrid.Children.IndexOf(movableElement)) as Image).Source = label.barKeyBitmap[0];

                    Mode mode;
                    if (label.barKey.All(char.IsDigit)) mode = Mode.Numeric;
                    else if (label.barKey.All(char.IsLetter)) mode = Mode.Alpha;
                    else if(label.barKey.All(char.IsLetterOrDigit)) mode = Mode.AlphaNumeric;
                    else
                    {
                        ErrorText.Visibility = Visibility.Visible;
                        ErrorText.Text = "Not a valid series to increment, try inputing something else";
                        return;
                    }

                    string tempKey = Increment(label.barKey, mode);

                    for (int i = 1; i < label.ArrRowColumn[0] * label.ArrRowColumn[1] * label.numOfPage; i++)
                    {
                        if (tempKey == "0") return;
                        System.Diagnostics.Debug.WriteLine(tempKey);
                        var result2 = writer.Write(tempKey);
                        label.barKeyBitmap.Add(result2.ToBitmap() as WriteableBitmap);
                        tempKey = Increment(tempKey,mode);
                    }
                }
            }
            catch(Exception x)
            {
                ErrorText.Visibility = Visibility.Visible;
                ErrorText.Text = x.Message;
            }
        }

        //TextBlock Pointer
        private void Textblock_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (!validState) return;

            movableElement = sender as UIElement;
            ptr = e.GetCurrentPoint(movableElement);
            textMovedValid = true;
        }

        private void TextBlock_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (!validState) return;

            e.Handled = true;
            textMovedValid = false;
        }

        private void Textblock_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (!validState) return;

            e.Handled = true;
            if (textMovedValid)
            {
                var textblock = movableElement as TextBlock;
                label.ArrTextLeftMargin[Convert.ToInt32(textblock.Name)] = textblock.Margin.Left;
                label.ArrTextTopMargin[Convert.ToInt32(textblock.Name)] = textblock.Margin.Top;
            }
            else if (imageMovedValid)
            {
                var image = movableElement as Image;
                label.ImageLeftMargin[(Convert.ToInt32(image.Name.Substring(6,image.Name.Length - 6))) - 1] = image.Margin.Left;
                label.ImageTopMargin[(Convert.ToInt32(image.Name.Substring(6, image.Name.Length - 6))) - 1] = image.Margin.Top;
            }
            else if (barcodeMovedValid)
            {
                var barcode = movableElement as Image;
                label.barLeftMargin = barcode.Margin.Left;
                label.barTopMargin = barcode.Margin.Top;
            }
            funcRegisterForPrinting();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            textMovedValid = false;
            imageMovedValid = false;
            ptr = null;
        }

        private void Textblock_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (!validState) return;

            e.Handled = true;
            if (!textMovedValid && !imageMovedValid && !barcodeMovedValid) return;
            try
            {
                PointerPoint ptrtemp = e.GetCurrentPoint(movableElement);
                if (!ptrtemp.IsInContact) return;
                if (textMovedValid)
                {
                    var textblock = movableElement as TextBlock;
                    double marginX = ptrtemp.Position.X - ptr.Position.X + textblock.Margin.Left;
                    double marginY = ptrtemp.Position.Y - ptr.Position.Y + textblock.Margin.Top;
                    e.Handled = true;
                    if (marginX < 0 && marginY < 0) textblock.Margin = new Thickness(textblock.Margin.Left, textblock.Margin.Top, 0, 0);
                    else if (marginX < 0 && marginY >= 0) textblock.Margin = new Thickness(textblock.Margin.Left, marginY, 0, 0);
                    else if (marginX >= 0 && marginY < 0) textblock.Margin = new Thickness(marginX, textblock.Margin.Top, 0, 0);
                    else if (marginX >= 0 && marginY >= 0) textblock.Margin = new Thickness(marginX, marginY, 0, 0);
                    label.ArrTextLeftMargin[Convert.ToInt32(textblock.Name)] = textblock.Margin.Left;
                    label.ArrTextTopMargin[Convert.ToInt32(textblock.Name)] = textblock.Margin.Top;
                }
                else if (imageMovedValid)
                {
                    var image = movableElement as Image;
                    double marginX = ptrtemp.Position.X - ptr.Position.X + image.Margin.Left;
                    double marginY = ptrtemp.Position.Y - ptr.Position.Y + image.Margin.Top;
                    e.Handled = true;
                    if (marginX < 0 && marginY < 0) image.Margin = new Thickness(image.Margin.Left, image.Margin.Top, 0, 0);
                    else if (marginX < 0 && marginY >= 0) image.Margin = new Thickness(image.Margin.Left, marginY, 0, 0);
                    else if (marginX >= 0 && marginY < 0) image.Margin = new Thickness(marginX, image.Margin.Top, 0, 0);
                    else if (marginX >= 0 && marginY >= 0) image.Margin = new Thickness(marginX, marginY, 0, 0);
               }
                else if (barcodeMovedValid)
                {
                    var barcode = movableElement as Image;
                    double marginX = ptrtemp.Position.X - ptr.Position.X + barcode.Margin.Left;
                    double marginY = ptrtemp.Position.Y - ptr.Position.Y + barcode.Margin.Top;
                    e.Handled = true;
                    if (marginX < 0 && marginY < 0) barcode.Margin = new Thickness(barcode.Margin.Left, barcode.Margin.Top, 0, 0);
                    else if (marginX < 0 && marginY >= 0) barcode.Margin = new Thickness(barcode.Margin.Left, marginY, 0, 0);
                    else if (marginX >= 0 && marginY < 0) barcode.Margin = new Thickness(marginX, barcode.Margin.Top, 0, 0);
                    else if (marginX >= 0 && marginY >= 0) barcode.Margin = new Thickness(marginX, marginY, 0, 0);
                }
            }
            catch
            {

            }
        }

        private void StateChangedOfUI(object sender, VisualStateChangedEventArgs e)
        {
            if (!validState) return;

            double phoneWidth = 350;
            double phoneHeight = 210;
            double desktopWidth = 500;
            double desktopHeight = 300;
            if (Window.Current.Bounds.Width >= 800)
            {
                ApplicationView view = ApplicationView.GetForCurrentView();

                view.TryEnterFullScreenMode();
                for (int i = 0; i < label.ArrTextLeftMargin.Count; i++)
                {
                    label.ArrTextLeftMargin[i] = (desktopWidth / phoneWidth) * label.ArrTextLeftMargin[i];
                    label.ArrTextTopMargin[i] = (desktopHeight / phoneHeight) * label.ArrTextTopMargin[i];
                    label.ArrTextFont[i] = (desktopHeight * desktopWidth) / (phoneHeight * phoneWidth) * label.ArrTextFont[i];

                }
                for (int i = 0; i < label.varImageCount; i++)
                {
                    label.ImageHeight[i] = (desktopHeight / phoneHeight) * label.ImageHeight[i];
                    label.ImageWidth[i] = (desktopWidth / phoneWidth) * label.ImageWidth[i];
                    label.ImageLeftMargin[i] = (desktopWidth / phoneWidth) * label.ImageLeftMargin[i];
                    label.ImageTopMargin[i] = (desktopHeight / phoneHeight) * label.ImageTopMargin[i];
                }
                if (label.barcodeValid)
                {
                    label.barHeight = (desktopHeight / phoneHeight) * label.barHeight;
                    label.barLeftMargin = (desktopWidth / phoneWidth) * label.barLeftMargin;
                    label.barTopMargin = (desktopHeight / phoneHeight) * label.barTopMargin;
                    label.barWidth = (desktopWidth / phoneWidth) * label.barWidth;
                }
            }
            else if (Window.Current.Bounds.Width > 0)
            {
                ApplicationView view = ApplicationView.GetForCurrentView();

                view.TryEnterFullScreenMode();
                for (int i = 0; i < label.ArrTextLeftMargin.Count; i++)
                    {
                        label.ArrTextLeftMargin[i] = (phoneWidth / desktopWidth) * label.ArrTextLeftMargin[i];
                        label.ArrTextTopMargin[i] = (phoneHeight / desktopHeight) * label.ArrTextTopMargin[i];
                        label.ArrTextFont[i] = (phoneHeight * phoneWidth) / (desktopHeight * desktopWidth) * label.ArrTextFont[i];
                    }
                    for(int i = 0; i < label.varImageCount; i++)
                    {
                        label.ImageHeight[i] = (phoneHeight / desktopHeight) * label.ImageHeight[i];
                        label.ImageWidth[i] = (phoneWidth / desktopWidth) * label.ImageWidth[i];
                        label.ImageLeftMargin[i] = (phoneWidth / desktopWidth) * label.ImageLeftMargin[i];
                        label.ImageTopMargin[i] = (phoneHeight / desktopHeight) * label.ImageTopMargin[i];
                    }
                    if (label.barcodeValid)
                    {
                        label.barHeight = (phoneHeight / desktopHeight) * label.barHeight;
                        label.barLeftMargin = (phoneWidth / desktopWidth) * label.barLeftMargin;
                        label.barTopMargin = (phoneHeight / desktopHeight) * label.barTopMargin;
                        label.barWidth = (phoneWidth / desktopWidth) * label.barWidth;
                    }
                }
                label.previewLabelGridWidth = PreviewLabelGrid.Width;
                label.previewLabelGridHeight = PreviewLabelGrid.Height;
                funcCreateLabel();
            }

        private void XAlignmentCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!validState) return;
            var combo = sender as ComboBox;
            if ("Left" == (combo.SelectedItem as ComboBoxItem).Content.ToString())
            {
                label.ArrXAlign[selectedIndex] = 0;
                (PreviewLabelGrid.Children.ElementAt(PreviewLabelGrid.Children.IndexOf(movableElement)) as TextBlock).HorizontalAlignment = HorizontalAlignment.Left;
            }
            else if("Center" == (combo.SelectedItem as ComboBoxItem).Content.ToString())
            {
                label.ArrXAlign[selectedIndex] = 1;
                (PreviewLabelGrid.Children.ElementAt(PreviewLabelGrid.Children.IndexOf(movableElement)) as TextBlock).HorizontalAlignment = HorizontalAlignment.Center;
            }
            else if("Right" == (combo.SelectedItem as ComboBoxItem).Content.ToString())
            {
                label.ArrXAlign[selectedIndex] = 2;
                (PreviewLabelGrid.Children.ElementAt(PreviewLabelGrid.Children.IndexOf(movableElement)) as TextBlock).HorizontalAlignment = HorizontalAlignment.Right;
            }
             (PreviewLabelGrid.Children.ElementAt(PreviewLabelGrid.Children.IndexOf(movableElement)) as TextBlock).Margin = new Thickness(0, (PreviewLabelGrid.Children.ElementAt(selectedIndex) as TextBlock).Margin.Top, 0, 0);
            label.ArrTextLeftMargin[selectedIndex] = 0;
        }

        private void YAlignmentCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!validState) return;
            var combo = sender as ComboBox;
            if ("Top" == (combo.SelectedItem as ComboBoxItem).Content.ToString())
            {
                label.ArrYAlign[selectedIndex] = 0;
                (PreviewLabelGrid.Children.ElementAt(PreviewLabelGrid.Children.IndexOf(movableElement)) as TextBlock).VerticalAlignment = VerticalAlignment.Top;
            }
            else if ("Center" == (combo.SelectedItem as ComboBoxItem).Content.ToString())
            {
                label.ArrYAlign[selectedIndex] = 1;
                (PreviewLabelGrid.Children.ElementAt(PreviewLabelGrid.Children.IndexOf(movableElement)) as TextBlock).VerticalAlignment = VerticalAlignment.Center;
            }
            else if ("Bottom" == (combo.SelectedItem as ComboBoxItem).Content.ToString())
            {
                label.ArrYAlign[selectedIndex] = 2;
                (PreviewLabelGrid.Children.ElementAt(PreviewLabelGrid.Children.IndexOf(movableElement)) as TextBlock).VerticalAlignment = VerticalAlignment.Bottom;
            }
            (PreviewLabelGrid.Children.ElementAt(PreviewLabelGrid.Children.IndexOf(movableElement)) as TextBlock).Margin = new Thickness((PreviewLabelGrid.Children.ElementAt(selectedIndex) as TextBlock).Margin.Left, 0, 0, 0);
            label.ArrTextTopMargin[selectedIndex] = 0;
        }

        //Image
        private void Image_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (!validState) return;

            e.Handled = true;
            imageMovedValid = true;
            movableElement = sender as UIElement;
            ptr = e.GetCurrentPoint(movableElement);
        }

        private void ComboboxImageStretechChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!validState) return;
            var combo = sender as ComboBox;
            if ("None" == (combo.SelectedItem as ComboBoxItem).Content.ToString())
            {
                label.ImageStretchAlignment[selectedIndex] = 0;
                (PreviewLabelGrid.Children.ElementAt(PreviewLabelGrid.Children.IndexOf(movableElement)) as Image).Stretch = Stretch.None;
            }
            else if ("Fill" == (combo.SelectedItem as ComboBoxItem).Content.ToString())
            {
                label.ImageStretchAlignment[selectedIndex] = 1;
                (PreviewLabelGrid.Children.ElementAt(PreviewLabelGrid.Children.IndexOf(movableElement)) as Image).Stretch = Stretch.Fill;
            }
            else if ("Uniform" == (combo.SelectedItem as ComboBoxItem).Content.ToString())
            {
                label.ImageStretchAlignment[selectedIndex] = 2;
                (PreviewLabelGrid.Children.ElementAt(PreviewLabelGrid.Children.IndexOf(movableElement)) as Image).Stretch = Stretch.Uniform;
            }
            else if ("Uniform To Fill" == (combo.SelectedItem as ComboBoxItem).Content.ToString())
            {
                label.ImageStretchAlignment[selectedIndex] = 3;
                (PreviewLabelGrid.Children.ElementAt(PreviewLabelGrid.Children.IndexOf(movableElement)) as Image).Stretch = Stretch.UniformToFill;
            }
        }

        private void ImageWidthChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (!validState) return;
            (PreviewLabelGrid.Children.ElementAt(PreviewLabelGrid.Children.IndexOf(movableElement)) as Image).Width = ImageWidth.Value;
            label.ImageWidth[selectedIndex] = ImageWidth.Value;
        }

        private void ImageHeightChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (!validState) return;
            (PreviewLabelGrid.Children.ElementAt(PreviewLabelGrid.Children.IndexOf(movableElement)) as Image).Height = ImageHeight.Value;
            label.ImageWidth[selectedIndex] = ImageHeight.Value;
        }

        private void ImageDeleteClicked(object sender, RoutedEventArgs e)
        {
            if (!validState) return;

            PreviewLabelGrid.Children.RemoveAt(PreviewLabelGrid.Children.IndexOf(movableElement));
            label.ImageBitmap.RemoveAt(selectedIndex);
            label.ImageHeight.RemoveAt(selectedIndex);
            label.ImageLeftMargin.RemoveAt(selectedIndex);
            label.ImageStretchAlignment.RemoveAt(selectedIndex);
            label.ImageTopMargin.RemoveAt(selectedIndex);
            label.ImageWidth.RemoveAt(selectedIndex);
            label.varImageCount--;
            ImageSettings.Visibility = Visibility.Collapsed;
            CreateLabelSettingsScrollViewer.Visibility = Visibility.Visible;
            funcRegisterForPrinting();
        }

        private void ImageSettingsOkClicked(object sender, RoutedEventArgs e)
        {
            if (!validState) return;

            ImageSettings.Visibility = Visibility.Collapsed;
            CreateLabelSettingsScrollViewer.Visibility = Visibility.Visible;
            funcRegisterForPrinting();
            GC.Collect();
        }

        private void Image_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (!validState) return;

            CreateLabelSettingsScrollViewer.Visibility = Visibility.Collapsed;
            MoreSettingsScrollView.Visibility = Visibility.Collapsed;
            BarcodeSettings.Visibility = Visibility.Collapsed;
            ImageSettings.Visibility = Visibility.Visible;

            var image = sender as Image;
            movableElement = sender as UIElement;
            selectedIndex = Convert.ToInt32(image.Name.Substring(6, image.Name.Length - 6)) - 1;
            ImageWidth.Value = label.ImageWidth[selectedIndex];
            ImageHeight.Value = label.ImageHeight[selectedIndex];
            if (label.ImageStretchAlignment[selectedIndex] == 0) ComboboxImageStretch.SelectedIndex = 0;
            else if (label.ImageStretchAlignment[selectedIndex] == 1) ComboboxImageStretch.SelectedIndex = 1;
            else if (label.ImageStretchAlignment[selectedIndex] == 2) ComboboxImageStretch.SelectedIndex = 2;
            else if (label.ImageStretchAlignment[selectedIndex] == 3) ComboboxImageStretch.SelectedIndex = 3;

        }

        private async void ImageButtonClicked(object sender, RoutedEventArgs e) 
        {
            if (!validState) return;
            try
            {
                /*
                string FileName = Textbox0.Text.ToString();
                StorageFolder _Folder = ApplicationData.Current.LocalFolder;*/

                var picker = new Windows.Storage.Pickers.FileOpenPicker();
                picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
                picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
                picker.FileTypeFilter.Add(".jpeg");
                picker.FileTypeFilter.Add(".png");

                StorageFile file = await picker.PickSingleFileAsync();
                if (".jpeg" == file.FileType) label.fileformat.Add(Label.FileFormat.Jpeg);
                else if (".png" == file.FileType) label.fileformat.Add(Label.FileFormat.Png);
                else return;
                if (file != null)
                {
                    WriteableBitmap bitmapImage = new WriteableBitmap(70,70);
                    FileRandomAccessStream stream = (FileRandomAccessStream)await file.OpenAsync(FileAccessMode.Read);
                    bitmapImage.SetSource(stream);

                    ++label.varImageCount;
                    label.ImageFileName.Add("Image_" + label.varImageCount);
                    label.ImageBitmap.Add(bitmapImage);
                    label.ImageWidth.Add(70);
                    label.ImageHeight.Add(70);
                    label.ImageStretchAlignment.Add(2);
                    label.ImageLeftMargin.Add(0);
                    label.ImageTopMargin.Add(0);
                   
                    funcCreateLabel();
                }
                funcRegisterForPrinting();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            catch
            {

            }

        }
        //Barcode
        private void BarcodeEnableCheckboxClicked(object sender, RoutedEventArgs e)
        {
            if (!validState) return;

            if (BarcodeEnableCheckbox.IsChecked == true)
            {
                label.barcodeValid = true;
                SelectTypeStack.Visibility = Visibility.Visible;
                BarcodeScrollViewer.Visibility = Visibility.Visible;
                BarcodeScrollViewer.IsEnabled = true;
                label.barHeight = 50;
                label.barWidth = 100;
                label.bartype = "AZTEC";
                label.barMode = false;
                label.barTopMargin = 50;
                label.barLeftMargin = 50;
                label.barKey = "Enter_Barcode_Key";
                BarcodeWidth.Value = label.barWidth;
                BarcodeHeight.Value = label.barHeight;
                funcCreateLabel();
            }
            else if(BarcodeEnableCheckbox.IsChecked == false)
            {
                label.barcodeValid = false;
                SelectTypeStack.Visibility = Visibility.Collapsed;
                BarcodeScrollViewer.IsEnabled = false;               
                UniformBarcode.IsChecked = true;
                IncrementalBarcode.IsChecked = false;

                funcCreateLabel();
            }
        }

        private void BarcodeSettingsOKClicked(object sender, RoutedEventArgs e)
        {
            if (!validState) return;
            if (ErrorText.Visibility == Visibility.Visible) return;
            BarcodeSettings.Visibility = Visibility.Collapsed;
            CreateLabelSettingsScrollViewer.Visibility = Visibility;
            funcBarcodeUpdate();
            funcRegisterForPrinting();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        private void BarcodeButtonClicked(object sender, RoutedEventArgs e)
        {
            if (!validState) return;
            CreateLabelSettingsScrollViewer.Visibility = Visibility.Collapsed;
            BarcodeSettings.Visibility = Visibility.Visible;
        }

        private void UniformBarcodeChecked(object sender, RoutedEventArgs e)
        {
            if (!validState || !label.barcodeValid) return;
            ErrorText.Visibility = Visibility.Collapsed;
            label.barMode = false;
        }

        private void BarcodeKeyTextChanged(object sender, TextChangedEventArgs e)
        {
            if (!validState || !label.barcodeValid) return;
            ErrorText.Visibility = Visibility.Collapsed;
            label.barKey = BarcodeKey.Text;
            try
            {
                if (!label.barKey.All(char.IsLetter) && !label.barKey.All(char.IsDigit) && label.barMode)
                {
                    if (!char.IsLetter(label.barKey[0]))
                    {
                        ErrorText.Visibility = Visibility.Visible;
                        ErrorText.Text = "Barcode key should start with an alphabet if it is an AplhaNemeric series.";
                    }
                }
            }
            catch
            {
                ErrorText.Visibility = Visibility.Visible;
                ErrorText.Text = "Not a valid series to increment, try inputing something else";
            }
        }

        private void BARCODEtypeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!validState || !label.barcodeValid) return;

            var combo = sender as ComboBox;
            if ("AZTEC" == (combo.SelectedItem as ComboBoxItem).Content.ToString()) label.bartype = "AZTEC";
            else if ("CODABAR" == (combo.SelectedItem as ComboBoxItem).Content.ToString()) label.bartype = "CODABAR";
            else if ("CODE 128" == (combo.SelectedItem as ComboBoxItem).Content.ToString()) label.bartype = "CODE_128";
            else if ("CODE 39" == (combo.SelectedItem as ComboBoxItem).Content.ToString()) label.bartype = "CODE_39";
            else if ("CODE 93" == (combo.SelectedItem as ComboBoxItem).Content.ToString()) label.bartype = "CODE_93";
            else if ("DATA MATRIX" == (combo.SelectedItem as ComboBoxItem).Content.ToString()) label.bartype = "DATA_MATRIX";
            else if ("EAN 13" == (combo.SelectedItem as ComboBoxItem).Content.ToString()) label.bartype = "EAN_13";
            else if ("EAN 8" == (combo.SelectedItem as ComboBoxItem).Content.ToString()) label.bartype = "EAN_8";
            else if ("ITF" == (combo.SelectedItem as ComboBoxItem).Content.ToString()) label.bartype = "ITF";
            else if ("MAXICODE" == (combo.SelectedItem as ComboBoxItem).Content.ToString()) label.bartype = "MAXICODE";
            else if ("MSI" == (combo.SelectedItem as ComboBoxItem).Content.ToString()) label.bartype = "MSI";
            else if ("PDF 417" == (combo.SelectedItem as ComboBoxItem).Content.ToString()) label.bartype = "PDF_417";
            else if ("PLESSEY" == (combo.SelectedItem as ComboBoxItem).Content.ToString()) label.bartype = "PLESSEY";
            else if ("QR CODE" == (combo.SelectedItem as ComboBoxItem).Content.ToString()) label.bartype = "QR_CODE";
            else if ("RSS 14" == (combo.SelectedItem as ComboBoxItem).Content.ToString()) label.bartype = "RSS_14";
            else if ("RSS EXPANDED" == (combo.SelectedItem as ComboBoxItem).Content.ToString()) label.bartype = "RSS_EXPANDED";
            else if ("UPC A" == (combo.SelectedItem as ComboBoxItem).Content.ToString()) label.bartype = "UPC_A";
            else if ("UPC E" == (combo.SelectedItem as ComboBoxItem).Content.ToString()) label.bartype = "UPC_E";
            else if ("UPC EAN EXTENSION" == (combo.SelectedItem as ComboBoxItem).Content.ToString()) label.bartype = "UPC_EAN_EXTENSION";

            funcBarcodeUpdate();
        }

        private void BarcodeWidthChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (!validState || !label.barcodeValid) return;
            label.barWidth = BarcodeWidth.Value;
            funcCreateLabel();
        }

        private void BarcodeHeightChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (!validState || !label.barcodeValid) return;
            label.barHeight = BarcodeHeight.Value;
            funcCreateLabel();
        }

        private void numOfLabelsChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!validState) return;
            label.numOfPage = numOfLabels.SelectedIndex + 1;
            if(label.barcodeValid) funcBarcodeUpdate();
            funcRegisterForPrinting();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void IncrementalBarcodeChecked(object sender, RoutedEventArgs e)
        {
            if (!validState || !label.barcodeValid) return;
            label.barMode = true;
            try
            {
                ErrorText.Visibility = Visibility.Collapsed;
                if (!label.barKey.All(char.IsLetter)&& !label.barKey.All(char.IsDigit) && label.barMode)
                {
                    if (!char.IsLetter(label.barKey[0]))
                    {
                        ErrorText.Visibility = Visibility.Visible;
                        ErrorText.Text = "Barcode key should start with an alphabet if it is an AplhaNemeric series.";
                    }
                }
            }
            catch
            {
                ErrorText.Visibility = Visibility.Visible;
                ErrorText.Text = "Not a valid series to increment, try inputing something else";
            }
        }

        private void Barcode_tapped(object sender, TappedRoutedEventArgs e)
        {
            if (!validState || !label.barcodeValid) return;
            CreateLabelSettingsScrollViewer.Visibility = Visibility.Collapsed;
            ImageSettings.Visibility = Visibility.Collapsed;
            MoreSettingsScrollView.Visibility = Visibility.Collapsed;
            BarcodeSettings.Visibility = Visibility.Visible;

            var barcode = sender as Image;
            movableElement = sender as UIElement;
            if (UniformBarcode.IsChecked == true || IncrementalBarcode.IsChecked == true)
            {
                if (!label.barMode)
                {
                    UniformBarcode.IsChecked = true;
                    IncrementalBarcode.IsChecked = false;
                }
                else if (label.barMode)
                {
                    UniformBarcode.IsChecked = false;
                    IncrementalBarcode.IsChecked = true;
                }
                BarcodeKey.Text = label.barKey;
                BarcodeWidth.Value = label.barWidth;
                BarcodeHeight.Value = label.barHeight;
                (Barcodetype.SelectedItem as ComboBoxItem).Content = label.bartype;
            }
        }

        private void Barcode_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (!validState || !label.barcodeValid) return;
            movableElement = sender as UIElement;
            ptr = e.GetCurrentPoint(movableElement);
            barcodeMovedValid = true;
        }

        private async void StoreToDatabase(object sender, RoutedEventArgs e)
        {
            if (!validState) return;
            if (string.IsNullOrEmpty(label.labelName))
            {
                return;
            }
            progressRing.IsActive = true;
            await Task.Delay(1000);
            label.Date = DateTime.Today.Date.ToString();
            label.Date = label.Date.Remove(11);
            label.Time = DateTime.Now.TimeOfDay.ToString();
            label.Time = label.Time.Remove(8);
            database.InsertLabel(label);
            progressRing.IsActive = false;
        }

        private async void StoreAndPrint(object sender, RoutedEventArgs e)
        {
            if (!validState) return;
            if (Windows.Graphics.Printing.PrintManager.IsSupported())
            {
                try
                {
                    funcRegisterForPrinting();
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    await Windows.Graphics.Printing.PrintManager.ShowPrintUIAsync();
                }
                catch
                {
                    ContentDialog noPrintingDialog = new ContentDialog()
                    {
                        Title = "Printing Error",
                        Content = "\nSorry, printing can't be proceed at this time.",
                        PrimaryButtonText = "Ok"
                    };
                    await noPrintingDialog.ShowAsync();
                }
            }
            else
            {
                ContentDialog noPrintingDialog = new ContentDialog()
                {
                    Title = "Printing not Supported",
                    Content = "\nSorry, printing is not supported on this device.",
                    PrimaryButtonText = "Ok"
                };
                await noPrintingDialog.ShowAsync();
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }
}
