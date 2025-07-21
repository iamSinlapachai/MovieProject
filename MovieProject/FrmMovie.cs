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

                        //ตึ้งค่าการแสดงรูปของ ListView 
                        if (lvShowAllMovie.SmallImageList == null)
                        {
                            lvShowAllMovie.SmallImageList = new ImageList();
                            lvShowAllMovie.SmallImageList.ImageSize = new Size(50, 50);
                            lvShowAllMovie.SmallImageList.ColorDepth = ColorDepth.Depth32Bit;

                        }
                        lvShowAllMovie.SmallImageList.Images.Clear();

                        //กำหนดรายละเอียดของ Colum ใน ListView
                        lvShowAllMovie.Columns.Add("รูปภาพยนต์", 120, HorizontalAlignment.Left);
                        lvShowAllMovie.Columns.Add("ชื่อภาพยนต์", 140, HorizontalAlignment.Left);
                        lvShowAllMovie.Columns.Add("รายละเอียดหนัง", 140, HorizontalAlignment.Left);
                        lvShowAllMovie.Columns.Add("วันที่ฉาย", 120, HorizontalAlignment.Left);
                        lvShowAllMovie.Columns.Add("ประเภทภาพยนต์", 120, HorizontalAlignment.Left);

                        lvShowSearchMovie.Columns.Add("รหัสภาพยนต์", 60, HorizontalAlignment.Left);
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
                            item.SubItems.Add(dataRow["movieDetail"].ToString());
                            item.SubItems.Add(dataRow["movieDate"].ToString());
                            item.SubItems.Add(dataRow["movieType"].ToString());


                            lvShowAllMovie.Items.Add(item);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("พบข้อผิดพลาด กรุณากรอกใหม่หรือติดต่อ IT: " + ex.Message, "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }


        private void FrmMovie_Load(object sender, System.EventArgs e)
        {
            resetPage();
        }


        private void resetPage()
        {
            // รีเซ็ต ListView ทั้งหมด
            getAllMovieToListView();

            // รีเซ็ตสถานะปุ่ม
            btUpdateMovie.Enabled = false;
            btDeleteMovie.Enabled = false;
            btSaveMovie.Enabled = true;

            // เคลียร์ TextBox และ ComboBox
            tbSearchMovie.Text = string.Empty;
            tbMovieName.Text = string.Empty;
            tbMovieDetail.Text = string.Empty;
            cbbMovieType.SelectedIndex = -1;

            // เคลียร์ Label ที่แสดงรหัสภาพยนต์
            lbMovieId.Text = string.Empty;

            // รีเซ็ต DateTimePicker และ NumericUpDown
            dtpMovieDate.Value = DateTime.Now;
            nudMovieHour.Value = 0;
            nudMovieMinute.Value = 0;            

            // รีเซ็ตรูปภาพ
            pcbMovieImage.Image = null;
            pcbMovieDirectorImage.Image = null;

            // เคลียร์ข้อมูล byte[] รูปภาพ
            movieImage = null;
            directorImage = null;          
            
        }

        private void btExit_Click(object sender, EventArgs e)
        {
            // ปิดฟอร์มนี้
            this.Close();

            // ถ้าต้องการปิดทั้งโปรแกรม
            // Application.Exit();
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

            else if (dtpMovieDate.Value.Date < DateTime.Today)
            {
                showWarningMessage("วันที่ออกฉายต้องเป็นวันปัจจุบันหรือล่วงหน้า");
            }
            else if (nudMovieHour.Value == 0 && nudMovieMinute.Value == 0)
            {
                showWarningMessage("โปรดใส่ความยาวของภาพยนตร์อย่างน้อย 1 นาที");
            }
            else if (nudMovieMinute.Value >= 60)
            {
                showWarningMessage("นาทีต้องน้อยกว่า 60 นาที");
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
                            sqlCommand.Parameters.Add("@movieType", SqlDbType.NVarChar, 150).Value = cbbMovieType.SelectedItem.ToString(); //save movie type
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
                        MessageBox.Show("พบข้อผิดพลาด กรุณากรอกใหม่หรือติดต่อ IT: " + ex.Message, "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private void btSearchMovie_Click(object sender, EventArgs e)
        {
            string searchText = tbSearchMovie.Text.Trim();

            // ตรวจสอบว่ามีการป้อนชื่อภาพยนตร์หรือยัง
            if (string.IsNullOrEmpty(searchText))
            {
                showWarningMessage("กรุณาป้อนชื่อภาพยนตร์ที่ต้องการค้นหา");
                return;
            }

            // เชื่อมต่อฐานข้อมูล
            using (SqlConnection sqlConnection = new SqlConnection(ShareResource.connectionString))
            {
                try
                {
                    sqlConnection.Open();

                    // ใช้ LIKE ใน SQL เพื่อค้นหาชื่อภาพยนตร์ที่มีคำค้น
                    string sql = "SELECT movieId, movieName FROM movie_tb WHERE movieName LIKE @searchText";

                    using (SqlCommand sqlCommand = new SqlCommand(sql, sqlConnection))
                    {
                        sqlCommand.Parameters.Add("@searchText", SqlDbType.NVarChar, 150).Value = "%" + searchText + "%";

                        using (SqlDataReader reader = sqlCommand.ExecuteReader())
                        {
                            //  ล้างรายการเก่าใน ListView
                            lvShowSearchMovie.Items.Clear();

                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    string movieId = reader["movieId"].ToString();
                                    string movieName = reader["movieName"].ToString();

                                    ListViewItem item1 = new ListViewItem(movieId);
                                    item1.SubItems.Add(movieName);

                                    lvShowSearchMovie.Items.Add(item1);
                                }
                            }
                            else
                            {
                                MessageBox.Show("ไม่พบภาพยนตร์ที่ค้นหา", "ผลการค้นหา", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("เกิดข้อผิดพลาดระหว่างค้นหา: " + ex.Message, "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void lvShowSearchMovie_ItemActivate(object sender, EventArgs e)
        {
            if (lvShowSearchMovie.SelectedItems.Count == 0)
            {
                showWarningMessage("กรุณาเลือกภาพยนตร์ที่ต้องการแสดงรายละเอียด");
                return;
            }

            string movieId = lvShowSearchMovie.SelectedItems[0].Text;

            using (SqlConnection sqlConnection = new SqlConnection(ShareResource.connectionString))
            {
                try
                {
                    sqlConnection.Open();

                    string sql = @"SELECT movieId, movieName, movieDetail, movieDate, movieHour, movieMinute, movieType,
                                  movieImage, movieDirectorImage
                           FROM movie_tb WHERE movieId = @movieId";

                    using (SqlCommand sqlCommand = new SqlCommand(sql, sqlConnection))
                    {
                        sqlCommand.Parameters.Add("@movieId", SqlDbType.Int).Value = int.Parse(movieId);

                        using (SqlDataReader reader = sqlCommand.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                lbMovieId.Text = reader["movieId"].ToString();
                                tbMovieName.Text = reader["movieName"].ToString();
                                tbMovieDetail.Text = reader["movieDetail"].ToString();
                                dtpMovieDate.Value = Convert.ToDateTime(reader["movieDate"]);
                                nudMovieHour.Value = Convert.ToInt32(reader["movieHour"]);
                                nudMovieMinute.Value = Convert.ToInt32(reader["movieMinute"]);

                                string movieType = reader["movieType"].ToString();
                                if (cbbMovieType.Items.Contains(movieType))
                                    cbbMovieType.SelectedItem = movieType;
                                else
                                    cbbMovieType.SelectedIndex = 0;

                                // โหลดรูปภาพโปสเตอร์
                                if (reader["movieImage"] != DBNull.Value)
                                {
                                    byte[] movieBytes = (byte[])reader["movieImage"];
                                    movieImage = movieBytes; // เก็บไว้ใช้ update หรือ delete
                                    pcbMovieImage.Image = convertByteArrayToImage(movieBytes);
                                }
                                else
                                {
                                    movieImage = null;
                                    pcbMovieImage.Image = null;
                                }

                                // โหลดรูปภาพผู้กำกับ
                                if (reader["movieDirectorImage"] != DBNull.Value)
                                {
                                    byte[] directorBytes = (byte[])reader["movieDirectorImage"];
                                    directorImage = directorBytes;
                                    pcbMovieDirectorImage.Image = convertByteArrayToImage(directorBytes);
                                }
                                else
                                {
                                    directorImage = null;
                                    pcbMovieDirectorImage.Image = null;
                                }

                                // ปุ่มจัดการสถานะ
                                btSaveMovie.Enabled = false;
                                btUpdateMovie.Enabled = true;
                                btDeleteMovie.Enabled = true;
                            }
                            else
                            {
                                MessageBox.Show("ไม่พบข้อมูลภาพยนตร์", "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("เกิดข้อผิดพลาด: " + ex.Message, "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void tbSearchMovie_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btSearchMovie_Click(sender, e);

                // ป้องกันเสียง 'ding' เมื่อกด Enter
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void btDeleteMovie_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(lbMovieId.Text))
            {
                MessageBox.Show("กรุณาเลือกภาพยนตร์ที่ต้องการลบ", "คำเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // ยืนยันก่อนลบ
            DialogResult result = MessageBox.Show("คุณต้องการลบข้อมูลภาพยนตร์นี้ใช่หรือไม่?", "ยืนยันการลบ", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                int movieId = int.Parse(lbMovieId.Text);

                using (SqlConnection sqlConnection = new SqlConnection(ShareResource.connectionString))
                {
                    try
                    {
                        sqlConnection.Open();

                        string sql = "DELETE FROM movie_tb WHERE movieId = @movieId";

                        using (SqlCommand sqlCommand = new SqlCommand(sql, sqlConnection))
                        {
                            sqlCommand.Parameters.Add("@movieId", SqlDbType.Int).Value = movieId;

                            int rowsAffected = sqlCommand.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("ลบข้อมูลเรียบร้อยแล้ว", "ผลการทำงาน", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                // รีเซ็ตหน้าจอเหมือนตอนเปิดหน้า
                                resetPage();
                            }
                            else
                            {
                                MessageBox.Show("ไม่พบข้อมูลภาพยนตร์ที่ต้องการลบ", "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("เกิดข้อผิดพลาดระหว่างลบ: " + ex.Message, "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            // else ไม่ต้องทำอะไรถ้าผู้ใช้กด No
        }

        private void btUpdateMovie_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(lbMovieId.Text))
            {
                showWarningMessage("กรุณาเลือกภาพยนตร์ที่ต้องการแก้ไข");
                return;
            }

            if (movieImage == null)
            {
                showWarningMessage("โปรดเลือกรูปภาพยนตร์");
                return;
            }

            if (directorImage == null)
            {
                showWarningMessage("โปรดเลือกรูปผู้กำกับ");
                return;
            }

            if (string.IsNullOrWhiteSpace(tbMovieName.Text))
            {
                showWarningMessage("โปรดกรอกชื่อภาพยนตร์");
                return;
            }

            if (string.IsNullOrWhiteSpace(tbMovieDetail.Text))
            {
                showWarningMessage("โปรดกรอกรายละเอียดภาพยนตร์");
                return;
            }

            if (dtpMovieDate.Value.Date < DateTime.Today)
            {
                showWarningMessage("วันที่ออกฉายต้องเป็นวันปัจจุบันหรือล่วงหน้า");
                return;
            }

            if (nudMovieHour.Value == 0 && nudMovieMinute.Value == 0)
            {
                showWarningMessage("โปรดระบุความยาวของภาพยนตร์อย่างน้อย 1 นาที");
                return;
            }

            if (nudMovieMinute.Value >= 60)
            {
                showWarningMessage("นาทีต้องน้อยกว่า 60 นาที");
                return;
            }

            if (cbbMovieType.SelectedIndex == -1)
            {
                showWarningMessage("โปรดเลือกประเภทภาพยนตร์");
                return;
            }

            // เริ่มอัปเดตข้อมูล
            using (SqlConnection sqlConnection = new SqlConnection(ShareResource.connectionString))
            {
                try
                {
                    sqlConnection.Open();

                    string sql = @"UPDATE movie_tb 
                           SET movieName = @movieName,
                               movieDetail = @movieDetail,
                               movieDate = @movieDate,
                               movieHour = @movieHour,
                               movieMinute = @movieMinute,
                               movieType = @movieType,
                               movieImage = @movieImage,
                               movieDirectorImage = @movieDirectorImage
                           WHERE movieId = @movieId";

                    using (SqlCommand sqlCommand = new SqlCommand(sql, sqlConnection))
                    {
                        sqlCommand.Parameters.Add("@movieId", SqlDbType.Int).Value = int.Parse(lbMovieId.Text.Trim());
                        sqlCommand.Parameters.Add("@movieName", SqlDbType.NVarChar, 150).Value = tbMovieName.Text.Trim();
                        sqlCommand.Parameters.Add("@movieDetail", SqlDbType.NVarChar, 500).Value = tbMovieDetail.Text.Trim();
                        sqlCommand.Parameters.Add("@movieDate", SqlDbType.Date).Value = dtpMovieDate.Value.Date;
                        sqlCommand.Parameters.Add("@movieHour", SqlDbType.Int).Value = (int)nudMovieHour.Value;
                        sqlCommand.Parameters.Add("@movieMinute", SqlDbType.Int).Value = (int)nudMovieMinute.Value;
                        sqlCommand.Parameters.Add("@movieType", SqlDbType.NVarChar, 150).Value = cbbMovieType.SelectedItem.ToString();
                        sqlCommand.Parameters.Add("@movieImage", SqlDbType.VarBinary, -1).Value = movieImage;
                        sqlCommand.Parameters.Add("@movieDirectorImage", SqlDbType.VarBinary, -1).Value = directorImage;

                        int rows = sqlCommand.ExecuteNonQuery();

                        if (rows > 0)
                        {
                            MessageBox.Show("แก้ไขข้อมูลเรียบร้อยแล้ว", "ผลการทำงาน", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            resetPage();
                        }
                        else
                        {
                            MessageBox.Show("ไม่สามารถแก้ไขข้อมูลได้ กรุณาลองใหม่", "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("เกิดข้อผิดพลาด: " + ex.Message, "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void lvShowAllMovie_ItemActivate(object sender, EventArgs e)
        {
            if (lvShowAllMovie.SelectedItems.Count == 0)
            {
                showWarningMessage("กรุณาเลือกภาพยนตร์ที่ต้องการแสดงรายละเอียด");
                return;
            }

            // สมมติ column ที่ 1 เป็นชื่อภาพยนตร์
            string movieName = lvShowAllMovie.SelectedItems[0].SubItems[1].Text;

            using (SqlConnection sqlConnection = new SqlConnection(ShareResource.connectionString))
            {
                try
                {
                    sqlConnection.Open();

                    string sql = @"SELECT TOP 1 movieId, movieName, movieDetail, movieDate, movieHour, movieMinute, movieType,
                                  movieImage, movieDirectorImage
                           FROM movie_tb
                           WHERE movieName = @movieName";

                    using (SqlCommand sqlCommand = new SqlCommand(sql, sqlConnection))
                    {
                        sqlCommand.Parameters.Add("@movieName", SqlDbType.NVarChar, 150).Value = movieName.Trim();

                        using (SqlDataReader reader = sqlCommand.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                lbMovieId.Text = reader["movieId"].ToString();
                                tbMovieName.Text = reader["movieName"].ToString();
                                tbMovieDetail.Text = reader["movieDetail"].ToString();
                                dtpMovieDate.Value = Convert.ToDateTime(reader["movieDate"]);
                                nudMovieHour.Value = Convert.ToInt32(reader["movieHour"]);
                                nudMovieMinute.Value = Convert.ToInt32(reader["movieMinute"]);

                                string movieType = reader["movieType"].ToString();
                                if (cbbMovieType.Items.Contains(movieType))
                                    cbbMovieType.SelectedItem = movieType;
                                else
                                    cbbMovieType.SelectedIndex = 0;

                                // โหลดภาพยนตร์
                                if (reader["movieImage"] != DBNull.Value)
                                {
                                    movieImage = (byte[])reader["movieImage"];
                                    pcbMovieImage.Image = convertByteArrayToImage(movieImage);
                                }
                                else
                                {
                                    movieImage = null;
                                    pcbMovieImage.Image = null;
                                }

                                // โหลดรูปผู้กำกับ
                                if (reader["movieDirectorImage"] != DBNull.Value)
                                {
                                    directorImage = (byte[])reader["movieDirectorImage"];
                                    pcbMovieDirectorImage.Image = convertByteArrayToImage(directorImage);
                                }
                                else
                                {
                                    directorImage = null;
                                    pcbMovieDirectorImage.Image = null;
                                }

                                btSaveMovie.Enabled = false;
                                btUpdateMovie.Enabled = true;
                                btDeleteMovie.Enabled = true;
                            }
                            else
                            {
                                MessageBox.Show("ไม่พบข้อมูลภาพยนตร์", "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("เกิดข้อผิดพลาด: " + ex.Message, "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
