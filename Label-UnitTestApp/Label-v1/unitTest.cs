using Windows.UI.Xaml.Media.Imaging;
using Xunit;

namespace Label_v1
{
    public class unitTest
    {
        Database database = new Database();
        Label label = new Label() { LB_id = 1, labelName= "Priyanshu", previewLabelGridHeight = 0, previewLabelGridWidth = 0, Date= "", Time = ""};

       [Fact]
        public void TestLoadDatabase()
        {
            try
            {
                database.LoadDatabase();
                Assert.True(true);
            }
            catch
            {
                Assert.False(false);
            }
        }

        [Fact] 
        public void TestInsertLabelToDatabase()
        {
            try
            {
                TestLoadDatabase();
                label = new Label();
                label.labelName = "Priyanshu";
                database.InsertLabel(label);
                Assert.True(true);

            }
            catch
            {
                Assert.False(false);
            }
        }

        [Fact]
        public async void TestGetLabelFromDatabase()
        {
            try
            {
                TestInsertLabelToDatabase();
                label = new Label();
                label.labelName = "Priyanshu";
                database.InsertLabel(label);
                Label tempLabel = await database.GetLabel(label);
                Assert.True(true);
            }
            catch
            {
                Assert.False(false);
            }
        }

        [Fact]
        public void TestLoadLabelDetailsFromDatabase()
        {
            try
            {
                TestInsertLabelToDatabase();
                database.LoadLabelDetail();
                Assert.True(true);
            }
            catch
            {
                Assert.False(false);
            }
        }  

        [Fact]
        public void TestDeleteLabel()
        {
            try
            {
                TestInsertLabelToDatabase();
                Label label = new Label() { LB_id = 1 };
                database.DeleteLabel(label);
                Assert.True(true);
            }
            catch
            {
                Assert.False(false);
            }
        }

        [Fact]
        public void TestGetLabelID()
        {
            try
            {
                TestLoadDatabase();
                database.GetLabelId();
                Assert.True(true);
            }
            catch
            {
                Assert.False(false);
            }
        }

        [Fact]
        public void TestWritableImageToStorage()
        {
            try
            {
                WriteableBitmap wb = new WriteableBitmap(40, 40);
                database.WriteableBitmapToStorageFile(wb, Label.FileFormat.Jpeg, "test");
                Assert.True(true);
            }
            catch
            {
                Assert.False(false);
            }
        }

        [Fact]
        public async void TestImagetoWritableBitmap()
        {
            try
            {
                TestWritableImageToStorage();
                await database.ImageNameToWriteableBitmap("test", 40, 40, Label.FileFormat.Jpeg);
                Assert.True(true);
            }
            catch
            {
                Assert.False(false);
            }
        }

        [Fact]
        public void TestLabelCopyConstructor()
        {
            try
            {
                Label temp = new Label(label);
                Assert.Equal(temp, label);
                Assert.True(true);
            }
            catch
            {
                Assert.False(false);
            }
        }
        
    }
}
