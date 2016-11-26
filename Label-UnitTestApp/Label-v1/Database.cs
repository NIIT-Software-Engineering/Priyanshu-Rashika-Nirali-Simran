using SQLitePCL;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace Label_v1
{
    class Database : IDisposable
    {
        public SQLiteConnection conn;
        public void LoadDatabase()
        {
            conn = new SQLiteConnection("LabeltestDB10.db");

            string labelDetailTable = @"Create table if not exists
                                 LabelDetail (LB_id integer primary key autoincrement not null,
                                        Name text not null,
                                        Row integer not null,
                                        Column integer not null,
                                        numPage integer not null,
                                        GridWidth real not null,
                                        GridHeight real not null,
                                        numTextBlocks integer not null,
                                        numImage integer not null,
                                        barcodeValid integer,
                                        Date text,
                                        Time text);";

            string textTable = @"Create table if not exists
                                Text (LB_id integer,
                                        stringText text,
                                        XAlign integer,
                                        YAlign integer,
                                        FontSize real,
                                        FontFamily text,
                                        leftMargin real,
                                        topMargin real,
                                        foreign key(LB_id) references LabelDetail(LB_id));";

            string imageTable = @"Create table if not exists
                                    Image (LB_id integer,
                                        Width real,
                                        Height real,
                                        stretchAlign integer,
                                        leftMargin real,
                                        topMargin real,
                                        fileFormat text,
                                        foreign key(LB_id) references LabelDetail(LB_id));";

            string barcodeTable1 = @"Create table if not exists
                                    Barcode1 (LB_id integer,
                                            Mode integer,
                                            Type text,
                                            Width real,
                                            Height real,
                                            Key text,
                                            leftMargin real,
                                            rightMargin real,
                                            foreign key(LB_id) references LabelDetail(LB_id));";

            using (var Statement = conn.Prepare(labelDetailTable)) Statement.Step();
            using (var Statement = conn.Prepare(textTable)) Statement.Step();
            using (var Statement = conn.Prepare(imageTable)) Statement.Step();
            using (var Statement = conn.Prepare(barcodeTable1)) Statement.Step();
            return;
        }

        public void InsertLabel(Label x)
        {
            try
            {
                string insertLabelDetail = @"Insert into LabelDetail(
                                        Name,
                                        Row,
                                        Column,
                                        numPage,
                                        GridWidth,
                                        GridHeight,
                                        numTextBlocks,
                                        numImage,
                                        barcodeValid,
                                        Date,
                                        Time) values(?,?,?,?,?,?,?,?,?,?,?);";
                using (var statement = conn.Prepare(insertLabelDetail))
                {
                    statement.Bind(1, x.labelName);
                    statement.Bind(2, x.ArrRowColumn[0]);
                    statement.Bind(3, x.ArrRowColumn[1]);
                    statement.Bind(4, x.numOfPage);
                    statement.Bind(5, x.previewLabelGridWidth);
                    statement.Bind(6, x.previewLabelGridHeight);
                    statement.Bind(7, x.varAddButton);
                    statement.Bind(8, x.varImageCount);
                    if (x.barcodeValid == true) statement.Bind(9, 1);
                    else statement.Bind(9, 0);
                    statement.Bind(10, x.Date);
                    statement.Bind(11, x.Time);
                    statement.Step();
                }
                //get label id
                x.LB_id = GetLabelId();
                if (x.LB_id == 0) return;

                string insertText = @"insert into Text(
                                        LB_id,
                                        stringText,
                                        XAlign,
                                        YAlign,
                                        FontSize,
                                        FontFamily,
                                        leftMargin,
                                        topMargin) values(?,?,?,?,?,?,?,?);";
                if (x.varAddButton > 0)
                {
                    for (int i = 0; i < x.ArrText.Count; i++)
                    {
                        using (var statement = conn.Prepare(insertText))
                        {

                            statement.Bind(1, x.LB_id);
                            statement.Bind(2, x.ArrText[i]);
                            statement.Bind(3, x.ArrXAlign[i]);
                            statement.Bind(4, x.ArrYAlign[i]);
                            statement.Bind(5, x.ArrTextFont[i]);
                            statement.Bind(6, x.ArrTextFontFamily[i]);
                            statement.Bind(7, x.ArrTextLeftMargin[i]);
                            statement.Bind(8, x.ArrTextTopMargin[i]);
                            statement.Step();
                        }
                    }
                }

                string insertImage = @"Insert into Image(
                                        LB_id,
                                        Width,
                                        Height,
                                        stretchAlign,
                                        leftMargin,
                                        topMargin,
                                        fileFormat) values(?,?,?,?,?,?,?);";
                if (x.varImageCount > 0)
                {
                    using (var statement = conn.Prepare(insertImage))
                    {
                        for (int i = 0; i < x.ImageBitmap.Count; i++)
                        {
                            statement.Bind(1, x.LB_id);
                            statement.Bind(2, x.ImageWidth[i]);
                            statement.Bind(3, x.ImageHeight[i]);
                            statement.Bind(4, x.ImageStretchAlignment[i]);
                            statement.Bind(5, x.ImageLeftMargin[i]);
                            statement.Bind(6, x.ImageTopMargin[i]);
                            statement.Bind(7, x.fileformat[i].ToString());

                            statement.Step();
                            WriteableBitmapToStorageFile(x.ImageBitmap[i], x.fileformat[i], (x.labelName + "_Image_" + i.ToString()));
                        }
                    }
                }
                string barcode1 = @"Insert into Barcode1(
                                            LB_id,
                                            Mode,
                                            Type,
                                            Width,
                                            Height,
                                            Key,
                                            leftMargin,
                                            rightMargin)values(?,?,?,?,?,?,?,?);";

                if (x.barcodeValid == true)
                {
                    using (var statement = conn.Prepare(barcode1))
                    {
                        statement.Bind(1, x.LB_id);
                        if (x.barMode == true) statement.Bind(2, 1);
                        else statement.Bind(2, 0);
                        statement.Bind(3, x.bartype);
                        statement.Bind(4, x.barWidth);
                        statement.Bind(5, x.barHeight);
                        statement.Bind(6, x.barKey);
                        statement.Bind(7, x.barLeftMargin);
                        statement.Bind(8, x.barTopMargin);

                        statement.Step();
                    }
                }
            }
            catch
            {

            }
        }
        public async Task<Label> GetLabel(Label x)
        {
            string LoadTextTable = @"select * from Text where LB_id = ?";

            if (x.varAddButton > 0)
            {
                using (var statement = conn.Prepare(LoadTextTable))
                {
                    statement.Bind(1, x.LB_id);
                    int i = 0;
                    while (statement.Step() == SQLiteResult.ROW)
                    {
                        x.ArrText.Insert(i, (string)statement[1]);
                        x.ArrXAlign.Insert(i, (int)(long)statement[2]);
                        x.ArrYAlign.Insert(i, (int)(long)statement[3]);
                        x.ArrTextFont.Insert(i, (double)statement[4]);
                        x.ArrTextFontFamily.Insert(i, (string)statement[5]);
                        x.ArrTextLeftMargin.Insert(i, (double)statement[6]);
                        x.ArrTextTopMargin.Insert(i, (double)statement[7]);

                        i++;
                    }
                }
            }
            if (x.varImageCount > 0)
            {
                string loadImage = @"select * from Image where LB_id = ?";

                using (var statement = conn.Prepare(loadImage))
                {
                    statement.Bind(1, x.LB_id);
                    int i = 0;
                    WriteableBitmap wb;
                    while (SQLiteResult.ROW == statement.Step())
                    {
                        x.ImageWidth.Insert(i, (double)statement[1]);
                        x.ImageHeight.Insert(i, (double)statement[2]);
                        x.ImageStretchAlignment.Insert(i, (double)(long)statement[3]);
                        x.ImageLeftMargin.Insert(i, (double)statement[4]);
                        x.ImageTopMargin.Insert(i, (double)statement[5]);
                        if ("Jpeg" == (string)statement[6]) x.fileformat.Insert(i,Label.FileFormat.Jpeg);
                        else if ("Png" == (string)statement[6]) x.fileformat.Insert(i,Label.FileFormat.Png);
                        else x.fileformat.Insert(i, Label.FileFormat.Jpeg);
                        wb = await ImageNameToWriteableBitmap((x.labelName + "_Image_" + i.ToString()), (int)x.ImageWidth[i], (int)x.ImageHeight[i], x.fileformat[i]);
                        x.ImageBitmap.Insert(i, wb);
                        i++;
                    }
                }
            }
            if (x.barcodeValid == true)
            {
                string loadBarcode1 = @"select * from Barcode1 where LB_id = ?";
                using (var statement = conn.Prepare(loadBarcode1))
                {
                    statement.Bind(1, x.LB_id);
                    if (SQLiteResult.ROW == statement.Step())
                    {
                        if (0 == (int)(long)statement[1]) x.barMode = false;
                        else x.barMode = true;
                        x.bartype = (string)statement[2];
                        x.barWidth = (double)statement[3];
                        x.barHeight = (double)statement[4];
                        x.barKey = (string)statement[5];
                        x.barLeftMargin = (double)statement[6];
                        x.barTopMargin = (double)statement[7];
                    }
                }
            }

            return x;
        }
        public ObservableCollection<Label> LoadLabelDetail()
        {
            string selectQuery = @"select * from LabelDetail";
            ObservableCollection<Label> label = new ObservableCollection<Label>();
            using(var statement = conn.Prepare(selectQuery))
            {
                while (SQLiteResult.ROW == statement.Step())
                {
                    
                    Label tempLabel = new Label()
                    {
                        LB_id = (long)statement[0],
                        labelName = (string)statement[1],
                        ArrRowColumn = new long[2] { (long)statement[2], (long)statement[3] },
                        numOfPage = (long)statement[4],
                        previewLabelGridWidth = (double)statement[5],
                        previewLabelGridHeight = (double)statement[6],
                        varAddButton = (long)statement[7],
                        varImageCount = (long)statement[8],
                    };
                    if (0 == (int)(long)statement[9]) tempLabel.barcodeValid = false;
                    else tempLabel.barcodeValid = true;
                    tempLabel.Date = (string)statement[10];
                    tempLabel.Time = (string)statement[11];

                    label.Add(tempLabel);
                }
                 
             }
            return label;
        }
        
        public long GetLabelId()
        {
            string query = @"select LB_id from LabelDetail where LB_id = (select max(LB_id) from LabelDetail);";
            long labelId = 0;
            using (var statement = conn.Prepare(query))
            {
                while (SQLiteResult.ROW == statement.Step())
                {
                    labelId = (long)statement[0];
                }
            }
            
            return labelId;
        }

        public bool DeleteLabel(Label label)
        {
            try
            {
                string deleteLabelDetail = @"delete from LabelDetail where LB_id = ?";
                using (var statement = conn.Prepare(deleteLabelDetail))
                {
                    statement.Bind(1, label.LB_id);
                    statement.Step();
                }
                string deleteText = @"delete from Text where LB_id = ?";
                if (label.varAddButton > 0)
                {
                    using (var statement = conn.Prepare(deleteText))
                    {
                        statement.Bind(1, label.LB_id);

                        while (statement.Step() == SQLiteResult.ROW) ;
                    }
                }
                string deleteImage = @"delete from Image where LB_id = ?";
                if (label.varImageCount > 0)
                {
                    using (var statement = conn.Prepare(deleteImage))
                    {
                        statement.Bind(1, label.LB_id);

                        while (statement.Step() == SQLiteResult.ROW) ;
                    }
                }
                if (label.barcodeValid)
                {
                    string deleteBarcode = @"delete from Barcode1 where LB_id = ?";
                    using (var statement = conn.Prepare(deleteBarcode))
                    {
                        statement.Bind(1, label.LB_id);

                        while (statement.Step() == SQLiteResult.ROW) ;
                    }
                }
                return true;
            } catch
            {
                return false;
            }
        }

        public async void WriteableBitmapToStorageFile(WriteableBitmap WB, Label.FileFormat fileFormatTemp, string FileName)
        {
            Guid BitmapEncoderGuid = BitmapEncoder.JpegEncoderId;
            switch (fileFormatTemp)
            {
                case Label.FileFormat.Jpeg:
                    FileName += ".jpeg";
                    BitmapEncoderGuid = BitmapEncoder.JpegEncoderId;
                    break;

                case Label.FileFormat.Png:
                    FileName += ".png";
                    BitmapEncoderGuid = BitmapEncoder.PngEncoderId;
                    break;

                case Label.FileFormat.Bmp:
                    FileName += ".bmp";
                    BitmapEncoderGuid = BitmapEncoder.BmpEncoderId;
                    break;

                case Label.FileFormat.Tiff:
                    FileName += ".tiff";
                    BitmapEncoderGuid = BitmapEncoder.TiffEncoderId;
                    break;

                case Label.FileFormat.Gif:
                    FileName += ".gif";
                    BitmapEncoderGuid = BitmapEncoder.GifEncoderId;
                    break;
            }
            StorageFolder folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Label_Images",CreationCollisionOption.OpenIfExists);
            StorageFile file = await folder.CreateFileAsync(FileName, CreationCollisionOption.OpenIfExists);
            using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoderGuid, stream);
                Stream pixelStream = WB.PixelBuffer.AsStream();
                byte[] pixels = new byte[pixelStream.Length];
                await pixelStream.ReadAsync(pixels, 0, pixels.Length);

                encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore,
                                    (uint)WB.PixelWidth,
                                    (uint)WB.PixelHeight,
                                    96.0,
                                    96.0,
                                    pixels);
                await encoder.FlushAsync();
            }
            return;
        }

        public async Task<WriteableBitmap> ImageNameToWriteableBitmap(string FileName, int width, int height, Label.FileFormat fileFormatTemp)
        {
            try
            {
                Guid BitmapEncoderGuid = BitmapEncoder.JpegEncoderId;

                switch (fileFormatTemp)
                {
                    case Label.FileFormat.Jpeg:
                        FileName += ".jpeg";
                        BitmapEncoderGuid = BitmapEncoder.JpegEncoderId;
                        break;

                    case Label.FileFormat.Png:
                        FileName += ".png";
                        BitmapEncoderGuid = BitmapEncoder.PngEncoderId;
                        break;

                    case Label.FileFormat.Bmp:
                        FileName += ".bmp";
                        BitmapEncoderGuid = BitmapEncoder.BmpEncoderId;
                        break;

                    case Label.FileFormat.Tiff:
                        FileName += ".tiff";
                        BitmapEncoderGuid = BitmapEncoder.TiffEncoderId;
                        break;

                    case Label.FileFormat.Gif:
                        FileName += ".gif";
                        BitmapEncoderGuid = BitmapEncoder.GifEncoderId;
                        break;
                }
                StorageFolder folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Label_Images",CreationCollisionOption.OpenIfExists);
                StorageFile file = await folder.GetFileAsync(FileName);
                WriteableBitmap bitmapImage = new WriteableBitmap(width, height);
                FileRandomAccessStream stream = (FileRandomAccessStream)await file.OpenAsync(FileAccessMode.Read);
                bitmapImage.SetSource(stream);
                return bitmapImage;
            }
            catch
            {
                StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync(FileName);
                WriteableBitmap bitmapImage = new WriteableBitmap(width, height);
                FileRandomAccessStream stream = (FileRandomAccessStream)await file.OpenAsync(FileAccessMode.Read);
                bitmapImage.SetSource(stream);
                return bitmapImage;
            }
         }

        public void Dispose()
        {
            ((IDisposable)conn).Dispose();
        }
    }
}
