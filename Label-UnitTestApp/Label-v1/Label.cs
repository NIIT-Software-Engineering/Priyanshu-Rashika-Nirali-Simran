using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace Label_v1
{
    public class Label
    {

        public enum FileFormat
        {
            Jpeg,
            Png,
            Bmp,
            Tiff,
            Gif
        }
        public ObservableCollection<FileFormat> fileformat = new ObservableCollection<FileFormat>();
        public double previewLabelGridWidth, previewLabelGridHeight;
        public long numOfPage = 1, LB_id;
        public string Date, Time;
        public long varAddButton = 0;
        public long[] ArrRowColumn = new long[2] {1,1};
        public string labelName;
        public ObservableCollection<string> ArrText = new ObservableCollection<string>();
        public ObservableCollection<int> ArrXAlign = new ObservableCollection<int>();
        public ObservableCollection<int> ArrYAlign = new ObservableCollection<int>();
        public ObservableCollection<double> ArrTextFont = new ObservableCollection<double>();
        public ObservableCollection<double> ArrTextLeftMargin = new ObservableCollection<double>();
        public ObservableCollection<double> ArrTextTopMargin = new ObservableCollection<double>();
        public ObservableCollection<string> ArrTextFontFamily = new ObservableCollection<string>();

        public long varImageCount = 0;
        public ObservableCollection<WriteableBitmap> ImageBitmap = new ObservableCollection<WriteableBitmap>();
        public ObservableCollection<string> ImageFileName = new ObservableCollection<string>();
        public ObservableCollection<double> ImageWidth = new ObservableCollection<double>();
        public ObservableCollection<double> ImageHeight = new ObservableCollection<double>();
        public ObservableCollection<double> ImageStretchAlignment = new ObservableCollection<double>();
        public ObservableCollection<double> ImageLeftMargin = new ObservableCollection<double>();
        public ObservableCollection<double> ImageTopMargin = new ObservableCollection<double>();

        public bool barcodeValid = false, barMode=false;
        public double barLeftMargin = 0, barTopMargin = 0;
        public string bartype = "All_1D";
        public string barKey = "Enter Barcode key";
        public double barWidth, barHeight;
        public ObservableCollection<WriteableBitmap> barKeyBitmap = new ObservableCollection<WriteableBitmap>();
        public Label()
        {

        }
        public Label(Label x)
        {
            x.LB_id = LB_id; 
            numOfPage = x.numOfPage;
            Date = x.Date;
            Time = x.Time;
            ArrRowColumn[0] = x.ArrRowColumn[0];
            ArrRowColumn[1] = x.ArrRowColumn[1];
            for(int i = 0; i < x.ArrText.Count; i++)
            {
                ArrText.Add(x.ArrText[i]);
            }
            for(int i = 0; i < x.ArrTextFont.Count; i++)
            {
                ArrTextFont.Add(x.ArrTextFont[i]);
            }
            for(int i = 0; i<x.ArrTextFontFamily.Count; i++)
            {
                ArrTextFontFamily.Add(x.ArrTextFontFamily[i]);
            }
            for(int i = 0; i< x.ArrTextLeftMargin.Count; i++)
            {
                ArrTextLeftMargin.Add(x.ArrTextLeftMargin[i]);
            }
            for(int i =0; i<x.ArrTextTopMargin.Count; i++)
            {
                ArrTextTopMargin.Add(x.ArrTextTopMargin[i]);
            }
            for(int i = 0; i<x.ArrXAlign.Count; i++)
            {
                ArrXAlign.Add(x.ArrXAlign[i]);
            }
            for(int i =0; i<x.ArrYAlign.Count; i++)
            {
                ArrYAlign.Add(x.ArrYAlign[i]);
            }
            barcodeValid = x.barcodeValid;
            barHeight = x.barHeight;
            barKey = x.barKey;
            for(int i = 0; i < x.barKeyBitmap.Count; i++)
            {
                barKeyBitmap.Add(x.barKeyBitmap[i]);
            }
            barLeftMargin = x.barLeftMargin;
            barMode = x.barMode;
            barTopMargin = x.barTopMargin;
            bartype = x.bartype;
            barWidth = x.barWidth;
            for(int i = 0; i<x.ImageBitmap.Count; i++)
            {
                ImageBitmap.Add(x.ImageBitmap[i]);
            }
            for (int i = 0; i < x.ImageFileName.Count; i++) ImageFileName.Add(x.ImageFileName[i]);
            for(int i = 0; i< x.ImageHeight.Count; i++) ImageHeight.Add(x.ImageHeight[i]);
            for(int i = 0; i<x.ImageLeftMargin.Count; i++) ImageLeftMargin.Add(x.ImageLeftMargin[i]);
            for(int i = 0; i< x.ImageStretchAlignment.Count; i++) ImageStretchAlignment.Add(x.ImageStretchAlignment[i]);
            for(int i = 0; i<x.ImageTopMargin.Count; i++) ImageTopMargin.Add(x.ImageTopMargin[i]);
            for(int i = 0; i<x.ImageWidth.Count; i++) ImageWidth.Add(x.ImageWidth[i]);
            labelName = x.labelName;
            previewLabelGridHeight = x.previewLabelGridHeight;
            previewLabelGridWidth = x.previewLabelGridWidth;
            varAddButton = x.varAddButton;
            varImageCount = x.varImageCount;
        }
    }
}
