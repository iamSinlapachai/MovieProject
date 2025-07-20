using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace MovieProject
{
    public partial class FrmMovie : Form
    {
        byte[] movieImage; // ตัวแปรสำหรับเก็บข้อมูลรูปภาพในรูปแบบ byte array for save to database
        byte[] directorImage; // ตัวแปรสำหรับเก็บข้อมูลรูปภาพในรูปแบบ byte array for save to database

        public FrmMovie()
        {
            InitializeComponent();
        }
        private Image convertByteArrayToImage(byte[] byteArrayIn)
        {
            if (byteArrayIn == null || byteArrayIn.Length == 0)
            {
                return null;
            }
            try
            {
                using (MemoryStream ms = new MemoryStream(byteArrayIn))
                {
                    return Image.FromStream(ms);
                }
            }
            catch (ArgumentException ex)
            {
                // อาจเกิดขึ้นถ้า byte array ไม่ใช่ข้อมูลรูปภาพที่ถูกต้อง
                Console.WriteLine("Error converting byte array to image: " + ex.Message);
                return null;
            }
        }
        private byte[] convertImageToByteArray(Image image, ImageFormat imageFormat)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, imageFormat);
                return ms.ToArray();
            }
        }

        private void showWarningMessage(string message)
        {
            MessageBox.Show(message, "คำเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }


        private void getAllMovieToListView()
        {
            //Connect String เพื่อติตต่อไปยังฐานข้อมูล
            //string connectionString = @"Server=DESKTOP-9U4FO0V\SQLEXPRESS;Database=coffee_cafe_db;Trusted_Connection=True;";

            //สร้าง connection ไปยังฐานข้อมูล
            using (SqlConnection sqlConnection = new SqlConnection(ShareResource.connectionString))
            {
                try
                {
                    sqlConnection.Open(); //open connectiong to db

                    //การทำงานกับตารางในฐานข้อมูล (SELECT, INSERT, UPDATE, DELETE)
                    //สร้างคำสั่ง SQL ให้ดึงข้อมูลจากตาราง product_db
                    string strSQL = "select movieId, movieName, movieDetail, movieDate, movieHour,  " +
                        "movieMinute, movieType, movieImage, " +
                        "movieDirectorImage from movie_tb";

                    //จัดการให้ SQL ทำงาน
                    using (SqlDataAdapter dataAdapter = new SqlDataAdapter(strSQL, sqlConnection))
                    {
                        //เอาข้อมูลที่ได้จาก strSQl ซึ่งเป็นก้อนใน dataAdapter มาทำให้เป็นตารางโดยใส่ไว้ใน dataTable
                        DataTable dataTable = new DataTable();
                        dataAdapter.Fill(dataTable);

                        //ตึ้งค่าทั่วไปของ ListView 
                        lvShowAllMovie.Items.Clear();
                        lvShowAllMovie.Columns.Clear();
                        lvShowAllMovie.FullRowSelect = true;
                        lvShowAllMovie.View = View.Details;

                        lvShowSearchMovie.Items.Clear();
                        lvShowSearchMovie.Columns.Clear();
                        lvShowSearchMovie.FullRowSelect = true;
                        lvShowSearchMovie.View = View.Details;

                        //กำหนดรายละเอียดของ Colum ใน ListView
                        lvShowAllMovie.Columns.Add("รูปภาพยนต์", 120, HorizontalAlignment.Left);
                        lvShowAllMovie.Columns.Add("ชื่อภาพยนต์", 140, HorizontalAlignment.Left);
                        lvShowAllMovie.Columns.Add("ชื่อผู้กำกับ", 140, HorizontalAlignment.Left);
                        lvShowAllMovie.Columns.Add("วันที่ฉาย", 120, HorizontalAlignment.Left);
                        lvShowAllMovie.Columns.Add("ประเภทภาพยนต์", 120, HorizontalAlignment.Left);

                        lvShowSearchMovie.Columns.Add("รหัสภาพยนต์", 80, HorizontalAlignment.Left);
                        lvShowSearchMovie.Columns.Add("ชื่อภาพยนต์", 140, HorizontalAlignment.Left);

                        //Loop วนเข้าไปใน DataTable
                        foreach (DataRow dataRow in dataTable.Rows)
                        {
                            ListViewItem item = new ListViewItem(); //create item for store data list
                            //put image in items
                            Image movieImage = null;
                            if (dataRow["movieImage"] != DBNull.Value)
                            {
                                byte[] imgByte = (byte[])dataRow["movieImage"];
                                movieImage = convertByteArrayToImage(imgByte);
                            }
                            string imageKey = null;
                            if (movieImage != null)
                            {
                                imageKey = $"movie_{dataRow["movieId"]}";
                                lvShowAllMovie.SmallImageList.Images.Add(imageKey, movieImage);
                                item.ImageKey = imageKey;
                            }
                            else
                            {
                                item.ImageIndex = -1;
                            }

                            item.SubItems.Add(dataRow["movieName"].ToString());
                            item.SubItems.Add(dataRow["movieDirectorName"].ToString());
                            item.SubItems.Add(dataRow["movieDate"].ToString());
                            item.SubItems.Add(dataRow["movieType"].ToString());


                            lvShowAllMovie.Items.Add(item);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("พบข้อผิดพลาด กรุณากรอกใหม่หรือติดต่อ IT : " + ex.Message);
                }
            }
        }


        private void FrmMovie_Load(object sender, System.EventArgs e)
        {
            resetPage();
        }


        private void resetPage()
        {
            getAllMovieToListView();
            btUpdateMovie.Enabled = false;
            btDeleteMovie.Enabled = false;
            cbbMovieType.SelectedIndex = 0;

            tbSearchMovie.Text = string.Empty;

            tbMovieName.Text = string.Empty;
            tbMovieDetail.Text = string.Empty;

            dtpMovieDate.Value = DateTime.Now;

            nudMovieHour.Value = 0;
            nudMovieMinute.Value = 0;
        }

        private void btExit_Click(object sender, EventArgs e)
        {

        }

        private void btResetMovie_Click(object sender, EventArgs e)
        {
            resetPage();
        }

        private void btSaveMovie_Click(object sender, EventArgs e)
        {
            // Validate input fields ad save the product to the database
            if (movieImage == null)
            {
                showWarningMessage("โปรดเลือกรูปภาพยนต์");                
            }
            else if (directorImage == null)
            {
                showWarningMessage("โปรดเลือกรูปผู้กำกับ");
            }
            else if (tbMovieName.Text.Length == 0)
            {
                showWarningMessage("โปรดใส่ชื่อภาพยนต์");
            }
            else if (tbMovieDetail.Text.Length == 0)
            {

                showWarningMessage("โปรดใส่รายละเอียดภาพยนต์");
            }

            else if (!dtpMovieDate.Checked)
            {

                showWarningMessage("โปรดใส่วันที่ออกฉาย");
                DateTime releaseDate = dtpMovieDate.Value;
                if (releaseDate.Date < DateTime.Today)
                {
                    MessageBox.Show("วันที่ออกฉายไม่ควรเป็นอดีต");

                }
            }
            else if (nudMovieHour.Value < 0 || nudMovieMinute.Value < 0)
            {
                showWarningMessage("โปรดใส่ความยาวของภาพยนต์เป็นตัวเลขที่ไม่ติดลบ");
            }
            else if (nudMovieHour.Value == 0 && nudMovieMinute.Value == 0)
            {
                showWarningMessage("โปรดใส่ความยาวของภาพยนต์");
            }

            // Check if the movie type is selected
            else if (cbbMovieType.SelectedIndex == -1)
            {
                showWarningMessage("โปรดเลือกประเภทภาพยนต์");
            }

            else
            {

                using (SqlConnection sqlConnection = new SqlConnection(ShareResource.connectionString))
                {
                    try
                    {
                        sqlConnection.Open(); //open connectiong to db                                           



                        SqlTransaction sqlTransaction = sqlConnection.BeginTransaction(); // Insert / Update / Delete data in transaction

                        //คำสั่ง SQL ให้เพิ่มข้อมูลลงในตาราง product_tb
                        string strSQL = "INSERT INTO movie_tb (movieName, movieDetail, movieDate, movieHour,  " +
                                         "movieMinute, movieType, movieImage, movieDirectorImage) " +
                                       "VALUES (@movieName, @movieDetail, @movieDate, @movieHour,  " +
                                         "@movieMinute, @movieType, @movieImage, @movieDirectorImage)";



                        //SQL Parameters to command SQL working
                        using (SqlCommand sqlCommand = new SqlCommand(strSQL, sqlConnection, sqlTransaction))
                        {
                            sqlCommand.Parameters.Add("@movieName", SqlDbType.NVarChar, 150).Value = tbMovieName.Text.Trim();
                            sqlCommand.Parameters.Add("@movieDetail", SqlDbType.NVarChar, 500).Value = tbMovieDetail.Text.Trim(); //save movie detail
                            sqlCommand.Parameters.Add("@movieDate", SqlDbType.Date).Value = dtpMovieDate.Value.Date; //save date                            
                            sqlCommand.Parameters.Add("@movieHour", SqlDbType.Int).Value = (int)nudMovieHour.Value; //save hour
                            sqlCommand.Parameters.Add("@movieMinute", SqlDbType.Int).Value = (int)nudMovieMinute.Value; //save minute   
                            sqlCommand.Parameters.Add("@movieType", SqlDbType.NVarChar, 50).Value = cbbMovieType.SelectedItem.ToString(); //save movie type
                            sqlCommand.Parameters.Add("@movieImage", SqlDbType.Image).Value = movieImage; //save image as byte array
                            sqlCommand.Parameters.Add("@movieDirectorImage", SqlDbType.Image).Value = directorImage; //save director image as byte array

                            // Execute the SQL command to insert data into the database
                            sqlCommand.ExecuteNonQuery();
                            sqlTransaction.Commit();

                            //messege box to show the result of the operation
                            MessageBox.Show("บันทึกข้อมูลเรียบร้อยแล้ว", "ผลการทำงาน", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            resetPage(); // Reset the form and clear all fields after saving
                        }

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("พบข้อผิดพลาด กรุณากรอกใหม่หรือติดต่อ IT : " + ex.Message);
                    }

                }
            }
            //after validate input fields show message box to confirm save and close the form and go back to FrmProductShow
        }

        private void btMovieImage_Click(object sender, EventArgs e)
        {
            //oepn file dialog to select image show only image files jpg, png

            //save the image to the database as byte array(Binary/Byte)
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = @"c:\";
            openFileDialog.Filter = "Image Files (*.Jpg;*.png)|*.jpg;*.png";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                // show the  image in the PictureBox 
                pcbMovieImage.Image = Image.FromFile(openFileDialog.FileName);
                // check the image format and convert the image to byte array
                if (pcbMovieImage.Image.RawFormat == ImageFormat.Jpeg)
                {
                    movieImage = convertImageToByteArray(pcbMovieImage.Image, ImageFormat.Jpeg);
                }
                else //if (pcbProImage.Image.RawFormat == ImageFormat.Png)
                {
                    movieImage = convertImageToByteArray(pcbMovieImage.Image, ImageFormat.Png);
                }
                //else
                //{
                //    MessageBox.Show("Please select a valid image file (JPG or PNG).", "Invalid File", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //    return;
                //}

            }
        }

        private void btMovieDirectorImage_Click(object sender, EventArgs e)
        {
            //oepn file dialog to select image show only image files jpg, png

            //save the image to the database as byte array(Binary/Byte)
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = @"c:\";
            openFileDialog.Filter = "Image Files (*.Jpg;*.png)|*.jpg;*.png";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                // show the  image in the PictureBox 
                pcbMovieDirectorImage.Image = Image.FromFile(openFileDialog.FileName);
                // check the image format and convert the image to byte array
                if (pcbMovieDirectorImage.Image.RawFormat == ImageFormat.Jpeg)
                {
                    directorImage = convertImageToByteArray(pcbMovieDirectorImage.Image, ImageFormat.Jpeg);
                }
                else //if (pcbProImage.Image.RawFormat == ImageFormat.Png)
                {
                    directorImage = convertImageToByteArray(pcbMovieDirectorImage.Image, ImageFormat.Png);
                }
                //else
                //{
                //    MessageBox.Show("Please select a valid image file (JPG or PNG).", "Invalid File", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //    return;
                //}

            }
        }
    }
}
