
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
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
    public sealed partial class Screen3_ListLabels : Page
    {
        private ObservableCollection<Label> Labels = new ObservableCollection<Label>();
        Label selectedLabel,tempLabel;
        Database database;
        PrintHelper printHelper;
        bool printUnregistered = true;
        public Screen3_ListLabels()
        {
            this.InitializeComponent();
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.FullScreen;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            database = e.Parameter as Database;
            database.LoadDatabase();
            Labels = database.LoadLabelDetail();
        }

        private async void ListView_itemClicked(object sender, ItemClickEventArgs e)
        {
            progressRing.IsActive = true;
            await Task.Delay(500);
            selectedLabel = await database.GetLabel((Label)e.ClickedItem);
            funcCreateLabel(selectedLabel);
            DeleteButton.Visibility = Visibility.Visible;
            PrintButton.Visibility = Visibility.Visible;
            progressRing.IsActive = false;
        }

        private void BackButtonClicked(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
               Frame.GoBack();
            }
        }

        void funcClearLabel()
        {
            PreviewLabelGrid.Children.Clear();
        }

        void funcCreateLabel(Label label)
        {
            funcClearLabel();
            for (int i = 0; i < label.varAddButton; i++)
            {
                var textblock = new TextBlock();
                textblock.Name = i.ToString();
                textblock.FontSize = label.ArrTextFont[i];
                textblock.FontFamily = new FontFamily(label.ArrTextFontFamily[i]);
                textblock.Foreground = new SolidColorBrush(Colors.Black);
                textblock.Text = label.ArrText[i];
                textblock.Margin = new Thickness(label.ArrTextLeftMargin[i], label.ArrTextTopMargin[i], 0, 0);

                if (label.ArrXAlign[i] == 0) textblock.HorizontalAlignment = HorizontalAlignment.Left;
                else if (label.ArrXAlign[i] == 1) textblock.HorizontalAlignment = HorizontalAlignment.Center;
                else if (label.ArrXAlign[i] == 2) textblock.HorizontalAlignment = HorizontalAlignment.Right;

                if (label.ArrYAlign[i] == 0) textblock.VerticalAlignment = VerticalAlignment.Top;
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
                    image.Width = label.ImageWidth[i];
                    image.Height = label.ImageHeight[i];
                    image.Margin = new Thickness(label.ImageLeftMargin[i], label.ImageTopMargin[i], 0, 0);
                    image.HorizontalAlignment = HorizontalAlignment.Left;
                    image.VerticalAlignment = VerticalAlignment.Top;
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
                barcode.Stretch = Stretch.Fill;
                barcode.VerticalAlignment = VerticalAlignment.Top;
                barcode.HorizontalAlignment = HorizontalAlignment.Left;
             
                PreviewLabelGrid.Children.Add(barcode);
                funcBarcodeUpdate(label);
                barcode.Source = label.barKeyBitmap[0];
            }

        }

        private void funcBarcodeUpdate(Label label)
        {
            try
            {
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
                }
                else if (label.barMode)
                {
                    var result = writer.Write(label.barKey);
                    label.barKeyBitmap.Add(result.ToBitmap() as WriteableBitmap);

                    Mode mode;
                    if (label.barKey.All(char.IsDigit)) mode = Mode.Numeric;
                    else if (label.barKey.All(char.IsLetter)) mode = Mode.Alpha;
                    else if (label.barKey.All(char.IsLetterOrDigit)) mode = Mode.AlphaNumeric;
                    else
                    {
                        return;
                    }

                    string tempKey = Increment(label.barKey, mode);

                    for (int i = 1; i < label.ArrRowColumn[0] * label.ArrRowColumn[1] * label.numOfPage; i++)
                    {
                        if (tempKey == "0") return;
                        System.Diagnostics.Debug.WriteLine(tempKey);
                        var result2 = writer.Write(tempKey);
                        label.barKeyBitmap.Add(result2.ToBitmap() as WriteableBitmap);
                        tempKey = Increment(tempKey, mode);
                    }
                }
            }
            catch
            {

            }
        }

        public enum Mode
        {
            AlphaNumeric = 1,
            Alpha = 2,
            Numeric = 3
        }
        public string Increment(string text, Mode mode)
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
                    foreach (char x in textArr)
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

                            }
                        }
                        else if (mode == Mode.Numeric)
                        {
                            textArr = textArr.Concat(new[] { '0' }).ToArray();
                            for (int z = textArr.Length - 2; z > 0; z--)
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

        private async void DeleteButtonClicked(object sender, RoutedEventArgs e)
        {
            progressRing.IsActive = true;
            await Task.Delay(1000);
            if (database.DeleteLabel(selectedLabel))
            {
                Labels.Remove(selectedLabel);
                PreviewLabelGrid.Children.Clear();
                progressRing.IsActive = false;
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private async void PrintButtonClicked(object sender, RoutedEventArgs e)
        {
            if (Windows.Graphics.Printing.PrintManager.IsSupported())
            {
                try
                {
                    progressRing.IsActive = true;
                    funcRegisterForPrinting();
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    await Windows.Graphics.Printing.PrintManager.ShowPrintUIAsync();
                    progressRing.IsActive = false;
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
                tempLabel = new Label(selectedLabel);
                printHelper = new PrintHelper(this);
                printHelper.RegisterForPrinting(tempLabel);

                printHelper.PreparePrintContent(new PageToPrint());
                printUnregistered = false;
            }
        }
    }
}
