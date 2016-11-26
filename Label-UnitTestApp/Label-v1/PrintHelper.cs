using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Graphics.Printing;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Printing;

namespace Label_v1
{
    class PrintHelper
    {
        protected PrintDocument printDocument;
        protected PrintManager printManager;
        protected IPrintDocumentSource printDocumentSource;
        protected Page scenarioPage;
        protected List<UIElement> printPreviewPages;
        protected event EventHandler PreviewPagesCreated;
        protected FrameworkElement firstPage;
        protected double ApplicationContentMarginLeft = 0.075;
        protected double ApplicationContentMarginTop = 0.03;
        protected static Label printlabel;
        public PrintHelper(Page scenarioPage)
        {
            this.scenarioPage = scenarioPage;
            printPreviewPages = new List<UIElement>();
        }

        public virtual void RegisterForPrinting(Label tempLabel)
        {
            try
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                printlabel = new Label();
                printlabel = tempLabel;
                printDocument = new PrintDocument();
                printDocumentSource = printDocument.DocumentSource;
                printDocument.Paginate += CreatePrintPreviewPages;
                printDocument.GetPreviewPage += GetPrintPreviewPage;
                printDocument.AddPages += AddPrintPages;

                printManager = PrintManager.GetForCurrentView();
                printManager.PrintTaskRequested += PrintTaskRequested;
            }
            catch
            {

            }
        }

        private void PrintTaskRequested(PrintManager sender, PrintTaskRequestedEventArgs args)
        {
            try
            {
                PrintTask printTask = null;
                printTask = args.Request.CreatePrintTask("Label Printing & Barocde Generator Printing Initiated", sourceRequested => {

                    IList<string> displayedOptions = printTask.Options.DisplayedOptions;
                    displayedOptions.Clear();
                    displayedOptions.Add(StandardPrintTaskOptions.Copies);
                    displayedOptions.Add(StandardPrintTaskOptions.MediaSize);
                    printTask.Options.MediaSize = PrintMediaSize.IsoA4;
                    printTask.Completed += async (s, args2) =>
                    {
                        if (args2.Completion == PrintTaskCompletion.Failed)
                        {
                            await scenarioPage.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {

                            });
                        }
                    };
                    sourceRequested.SetSource(printDocumentSource);
                });
            }
            catch
            {

            }
        }

        private void AddPrintPages(object sender, AddPagesEventArgs e)
        {
            try
            {
                for (int i = 0; i < printPreviewPages.Count; i++)
                {
                    printDocument.AddPage(printPreviewPages[i]);
                }
                PrintDocument printDoc = (PrintDocument)sender;
                printDoc.AddPagesComplete();
            }
            catch
            {

            }
        }

        private void GetPrintPreviewPage(object sender, GetPreviewPageEventArgs e)
        {
            try
            {
                PrintDocument printDoc = (PrintDocument)sender;
                printDoc.SetPreviewPage(e.PageNumber, printPreviewPages[e.PageNumber - 1]);
            }
            catch
            {

            }
        }

        private void CreatePrintPreviewPages(object sender, PaginateEventArgs e)
        {
                printPreviewPages.Clear();
                PrintTaskOptions printingOptions = e.PrintTaskOptions;
                PrintPageDescription pageDescription = printingOptions.GetPageDescription(0);
            for (int z = 0; z < printlabel.ArrTextLeftMargin.Count; z++)
            {
                printlabel.ArrTextLeftMargin[z] = (pageDescription.ImageableRect.Width / (printlabel.previewLabelGridWidth * printlabel.ArrRowColumn[1])) * printlabel.ArrTextLeftMargin[z] + 2;
                printlabel.ArrTextTopMargin[z] = (pageDescription.ImageableRect.Height / (printlabel.previewLabelGridHeight * printlabel.ArrRowColumn[0])) * printlabel.ArrTextTopMargin[z] + 2;
                printlabel.ArrTextFont[z] = (pageDescription.ImageableRect.Width * pageDescription.ImageableRect.Height) / (printlabel.previewLabelGridWidth * printlabel.previewLabelGridHeight * printlabel.ArrRowColumn[0] * printlabel.ArrRowColumn[1]) * printlabel.ArrTextFont[z] + 5;
            }
            for (int z = 0; z < printlabel.ImageLeftMargin.Count; z++)
            {
                printlabel.ImageHeight[z] = (pageDescription.ImageableRect.Height / (printlabel.previewLabelGridHeight * printlabel.ArrRowColumn[0])) * printlabel.ImageHeight[z] + 2;
                printlabel.ImageWidth[z] = (pageDescription.ImageableRect.Width / (printlabel.previewLabelGridWidth * printlabel.ArrRowColumn[1])) * printlabel.ImageWidth[z] + 2;
                printlabel.ImageLeftMargin[z] = (pageDescription.ImageableRect.Width / (printlabel.previewLabelGridWidth * printlabel.ArrRowColumn[1])) * printlabel.ImageLeftMargin[z] + 2;
                printlabel.ImageTopMargin[z] = (pageDescription.ImageableRect.Height / (printlabel.previewLabelGridHeight * printlabel.ArrRowColumn[0])) * printlabel.ImageTopMargin[z] + 2;
            }
            printlabel.barHeight = (pageDescription.ImageableRect.Height / (printlabel.previewLabelGridHeight * printlabel.ArrRowColumn[0])) * printlabel.barHeight + 2;
            printlabel.barLeftMargin = (pageDescription.ImageableRect.Width / (printlabel.previewLabelGridWidth * printlabel.ArrRowColumn[1])) * printlabel.barLeftMargin + 2;
            printlabel.barTopMargin = (pageDescription.ImageableRect.Height / (printlabel.previewLabelGridHeight * printlabel.ArrRowColumn[0])) * printlabel.barTopMargin + 2;
            printlabel.barWidth = (pageDescription.ImageableRect.Width / (printlabel.previewLabelGridWidth * printlabel.ArrRowColumn[1])) * printlabel.barWidth + 2;
            //Implement PageToPrint
            int temp = 1;
            while (temp < 201)
            {
                if (temp <= printlabel.numOfPage) AddOnePrintPage(pageDescription);
                else break;
                temp++;
            }
            if (PreviewPagesCreated != null)
            {
                PreviewPagesCreated.Invoke(printPreviewPages, null);
            }
            PrintDocument printDoc = (PrintDocument)sender;
            printDoc.SetPreviewPageCount(printPreviewPages.Count, PreviewPageCountType.Final);

        }

        public void AddOnePrintPage(PrintPageDescription pageDescription)
        {
            try {
            FrameworkElement page = firstPage;
            Grid PrintableArea = (Grid)page.FindName("PrintableArea");
            int incrementalBarCount = 0;
                //ROWS
                PrintableArea.RowDefinitions.Clear();
                for (int i = 0; i < printlabel.ArrRowColumn[0]; i++)
                {
                    RowDefinition r = new RowDefinition();
                    PrintableArea.RowDefinitions.Add(r);
                }
                foreach (RowDefinition row in PrintableArea.RowDefinitions)
                {
                    row.Height = new GridLength(1, GridUnitType.Star);
                }

                //COLUMNS
                PrintableArea.ColumnDefinitions.Clear();
                for (int i = 0; i < printlabel.ArrRowColumn[1]; i++)
                {
                    ColumnDefinition c = new ColumnDefinition();
                    PrintableArea.ColumnDefinitions.Add(c);
                }
                foreach (ColumnDefinition col in PrintableArea.ColumnDefinitions)
                {
                    col.Width = new GridLength(1, GridUnitType.Star);
                }
                PrintableArea.Children.Clear();

                for (int i = 0; i < PrintableArea.RowDefinitions.Count; i++)
                {
                    for (int j = 0; j < PrintableArea.ColumnDefinitions.Count; j++)
                    {
                            for (int z = 0; z < printlabel.varAddButton; z++)
                            {
                                var textblock = new TextBlock();
                                textblock.Name = z.ToString();
                                textblock.FontSize = printlabel.ArrTextFont[z];
                                textblock.FontFamily = new FontFamily(printlabel.ArrTextFontFamily[z]);
                                textblock.Foreground = new SolidColorBrush(Colors.Black);
                                textblock.Text = printlabel.ArrText[z];
                                textblock.Margin = new Thickness(printlabel.ArrTextLeftMargin[z], printlabel.ArrTextTopMargin[z], 0, 0);
                                textblock.SetValue(Grid.RowProperty, i);
                                textblock.SetValue(Grid.ColumnProperty, j);

                                if (printlabel.ArrXAlign[z] == 0) textblock.HorizontalAlignment = HorizontalAlignment.Left;
                                else if (printlabel.ArrXAlign[z] == 1) textblock.HorizontalAlignment = HorizontalAlignment.Center;
                                else if (printlabel.ArrXAlign[z] == 2) textblock.HorizontalAlignment = HorizontalAlignment.Right;

                                if (printlabel.ArrYAlign[z] == 0) textblock.VerticalAlignment = VerticalAlignment.Top;
                                else if (printlabel.ArrYAlign[z] == 1) textblock.VerticalAlignment = VerticalAlignment.Center;
                                else if (printlabel.ArrYAlign[z] == 2) textblock.VerticalAlignment = VerticalAlignment.Bottom;

                                PrintableArea.Children.Add(textblock);

                            }
                            if (printlabel.varImageCount != 0)
                            {

                                for (int y = 0; y < printlabel.varImageCount; y++)
                                {
                                    var image = new Image();
                                    image.Source = printlabel.ImageBitmap[y];
                                    image.Width = printlabel.ImageWidth[y];
                                    image.Height = printlabel.ImageHeight[y];
                                    image.Margin = new Thickness(printlabel.ImageLeftMargin[y], printlabel.ImageTopMargin[y], 0, 0);
                                    image.HorizontalAlignment = HorizontalAlignment.Left;
                                    image.VerticalAlignment = VerticalAlignment.Top;
                                    image.SetValue(Grid.RowProperty, i);
                                    image.SetValue(Grid.ColumnProperty, j);
                                    if (printlabel.ImageStretchAlignment[y] == 0) image.Stretch = Stretch.None;
                                    else if (printlabel.ImageStretchAlignment[y] == 1) image.Stretch = Stretch.Fill;
                                    else if (printlabel.ImageStretchAlignment[y] == 2) image.Stretch = Stretch.Uniform;
                                    else if (printlabel.ImageStretchAlignment[y] == 3) image.Stretch = Stretch.UniformToFill;

                                    PrintableArea.Children.Add(image);
                                }
                            }
                            if (printlabel.barcodeValid)
                            {
                                var barcode = new Image();
                                barcode.Name = "BarcodeImage";
                                barcode.Width = printlabel.barWidth;
                                if (!printlabel.barMode) barcode.Source = printlabel.barKeyBitmap[0];
                                else if (printlabel.barMode) barcode.Source = printlabel.barKeyBitmap[incrementalBarCount];
                                barcode.Height = printlabel.barHeight;
                                barcode.Margin = new Thickness(printlabel.barLeftMargin, printlabel.barTopMargin, 0, 0);
                                barcode.Stretch = Stretch.Fill;
                                barcode.SetValue(Grid.RowProperty, i);
                                barcode.SetValue(Grid.ColumnProperty, j);
                                barcode.VerticalAlignment = VerticalAlignment.Top;
                                barcode.HorizontalAlignment = HorizontalAlignment.Left;

                                PrintableArea.Children.Add(barcode);
                                incrementalBarCount++;

                            }
                    }
                }
                printPreviewPages.Add(page);
                return;
            }
            catch
            {

            }
        }

        public async Task ShowPrintUIAsync()
        {
            try
            {
                await PrintManager.ShowPrintUIAsync();
            }
            catch (Exception e)
            {
                ContentDialog noprinting = new ContentDialog()
                {
                    Title = "Error in Printing",
                    Content = e.Message,
                    PrimaryButtonText = "Ok"
                };
                await noprinting.ShowAsync();
            }

        }

        public virtual void PreparePrintContent(Page page)
        {
            if (firstPage == null)
            {

                firstPage = page;

            }

        }

        public virtual void UnRegisterForPrinting()
        {
            try
            {
                if (printDocument == null) return;
                printDocument.Paginate -= CreatePrintPreviewPages;
                printDocument.GetPreviewPage -= GetPrintPreviewPage;
                printDocument.AddPages -= AddPrintPages;

                PrintManager printman = PrintManager.GetForCurrentView();
                printman.PrintTaskRequested -= PrintTaskRequested;
                printlabel = null;
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            catch
            {

            }
        }
    }
}
