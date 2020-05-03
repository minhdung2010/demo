using QuanLyQuanKaraoke.DAO;
using QuanLyQuanKaraoke.DTO;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace QuanLyQuanKaraoke
{
    public partial class fTableManager : Form
    {
        private Account loginAccount;

        public Account LoginAccount
        {
            get
            {
                return loginAccount;
            }

            set
            {
                loginAccount = value;

                ChangeAccount(loginAccount.Type);
            }
        }

        public fTableManager(Account acc)
        {
            InitializeComponent();

            this.LoginAccount = acc;//truyền tài khoản đăng nhập

            LoadRoom();

            LoadCategory();

            LoadComboboxRoom(cbSwtichRoom);
        }

        #region Method

        void ChangeAccount(int type)//khi nào là admin thì mới hiển thị form admin
        {
            adminToolStripMenuItem.Enabled = type == 1;

            thôngTinTàiKhoảnToolStripMenuItem.Text += " (" + LoginAccount.DisplayName + ") ";
        }

        void LoadCategory()//lấy ra danh sách loại thức ăn
        {
            List<Category> listCategory = CategoryDAO.Instance.GetListCategory();

            cbCategory.DataSource = listCategory;

            cbCategory.DisplayMember = "Name";
        }

        void LoadFoodListByCategoryID(int id)//lấy ra danh sách thức ăn từ loại thức ăn
        {
            List<Food> listFood = FoodDAO.Instance.GetFoodByCategoryID(id);

            cbFood.DataSource = listFood;

            cbFood.DisplayMember = "Name";
        }

        void LoadRoom()// Load danh sách phòng
        {
            flpRoom.Controls.Clear();// làm mới khi click vào phòng

            List<Room> roomList = RoomDAO.Instance.LoadRoomList();//lấy ra RoomList

            foreach(Room item in roomList)
            {
                Button btn = new Button() {Width = RoomDAO.RoomWidtth, Height = RoomDAO.RoomHeight };// tạo ra buttton co kích thước

                btn.Text = item.Name + Environment.NewLine + item.Status;

                btn.Click += btn_Click;

                btn.Tag = item;

                switch (item.Status)
                {
                    case "Co nguoi":
                        btn.BackColor = Color.LightPink;//màu LightPink là phòng đang có người
                        break;
                    default:
                        btn.BackColor = Color.Aqua;//Aqua là phòng đang trống
                        break;
                }

                flpRoom.Controls.Add(btn);
            }
        }

        void ShowBill(int id)//hiển thị hóa đơn
        {

            lsvBill.Items.Clear();

            List<QuanLyQuanKaraoke.DTO.Menu> listBillInfo = MenuDAO.Instance.GetListMenuByRoom(id);

            float totalPrice = 0;

            foreach(QuanLyQuanKaraoke.DTO.Menu item in listBillInfo)//hiển thị các thông tin cần thiết cho hóa đơn
            {
                ListViewItem lsvItem = new ListViewItem(item.FoodName.ToString());
                lsvItem.SubItems.Add(item.Count.ToString());
                lsvItem.SubItems.Add(item.Price.ToString());
                lsvItem.SubItems.Add(item.TotalPrice.ToString());
                totalPrice += item.TotalPrice;
                lsvBill.Items.Add(lsvItem);
            }
            CultureInfo culture = new CultureInfo("vi-VN");//đơn bị tiền tệ là VNĐ

            Thread.CurrentThread.CurrentCulture = culture;

            txbTotalPrice.Text = totalPrice.ToString("c");         
        }

        void LoadComboboxRoom(ComboBox cb)
        {
            cb.DataSource = RoomDAO.Instance.LoadRoomList();
            cb.DisplayMember = "Name";
        }
        #endregion

        #region Events

        void btn_Click(object sender, EventArgs e)
        {
            int RoomID = ((sender as Button).Tag as Room).ID;
            lsvBill.Tag = ((sender as Button).Tag);
            ShowBill(RoomID);

        }

        private void đăngXuấtToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void thôngTinCáNhânToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fAccountProfile f = new fAccountProfile(LoginAccount);
            f.ShowDialog();
        }

        private void adminToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fAdmin f = new fAdmin();
            f.InsertFood += f_InsertFood;
            f.DeleteFood += f_DeleteFood;
            f.UpdateFood += f_UpdateFood;
            f.ShowDialog();
        }

        void f_UpdateFood(object sender, EventArgs e)
        {
            LoadFoodListByCategoryID((cbCategory.SelectedItem as Category).ID);
            if (lsvBill.Tag != null)
                ShowBill((lsvBill.Tag as Room).ID);
        }

        void f_InsertFood(object sender, EventArgs e)
        {
            LoadFoodListByCategoryID((cbCategory.SelectedItem as Category).ID);
            if(lsvBill.Tag != null)
                ShowBill((lsvBill.Tag as Room).ID);
        }

        void f_DeleteFood(object sender, EventArgs e)
        {
            LoadFoodListByCategoryID((cbCategory.SelectedItem as Category).ID);
            if (lsvBill.Tag != null)
                ShowBill((lsvBill.Tag as Room).ID);
            LoadRoom();
        }
        
        private void cbCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            int id = 0;

            ComboBox cb = sender as ComboBox;

            if (cb.SelectedItem == null)
                return;

            Category selected = cb.SelectedItem as Category;

            id = selected.ID;

            LoadFoodListByCategoryID(id);
        }

        private void btnAddFood_Click(object sender, EventArgs e)
        {
            Room room = lsvBill.Tag as Room;

            if(room == null)
            {
                MessageBox.Show("Hãy chọn bàn");
                return;
            }
            int idBill = BillDAO.Instance.GetUncheckBillIDByRoomID(room.ID);
            int foodID = (cbFood.SelectedItem as Food).ID;
            int count = (int)nmFoodCount.Value;

            if(idBill == -1)
            {
                BillDAO.Instance.InsertBill(room.ID);
                BillInfoDAO.Instance.InsertBillInfo(BillDAO.Instance.GetMaxIDBill(), foodID, count);
            }
            else
            {
                BillInfoDAO.Instance.InsertBillInfo(idBill, foodID, count);
            }

            ShowBill(room.ID);
            LoadRoom();
        }

        private void btnCheckOut_Click(object sender, EventArgs e)
        {
            Room room = lsvBill.Tag as Room;

            int idBill = BillDAO.Instance.GetUncheckBillIDByRoomID(room.ID);

            int discount = (int)nmDiscount.Value;

            double totalPrice = Convert.ToDouble(txbTotalPrice.Text.Split(',')[0]);

            double finalTotalPrice = totalPrice - (totalPrice / 100) * discount;

            if(idBill != -1)
            {
                if (MessageBox.Show(string.Format("Bạn có chắc chắn thanh toán hóa đơn cho phòng {0}\n Tổng tiền - (Tổng tiền / 100) x  Giảm giá\n => {1} - ({1} / 100) x {2} = {3}" ,room.Name, totalPrice,discount,finalTotalPrice),"Thông báo",MessageBoxButtons.OKCancel) == System.Windows.Forms.DialogResult.OK)
                {
                    BillDAO.Instance.CheckOut(idBill, discount);
                    ShowBill(room.ID);
                    LoadRoom();
                }
            }
        }

        private void btnSwitchRoom_Click(object sender, EventArgs e)
        {
            int id1 = (lsvBill.Tag as Room).ID;

            int id2 = (cbSwtichRoom.SelectedItem as Room).ID;
            if (MessageBox.Show(string.Format("Bạn có muốn chuyển từ bàn {0} sang bàn {1}", (lsvBill.Tag as Room).Name, (cbSwtichRoom.SelectedItem as Room).Name), "Thông báo", MessageBoxButtons.OKCancel) == System.Windows.Forms.DialogResult.OK)
            {


                RoomDAO.Instance.SwitchRoom(id1, id2);

                LoadRoom();
            }
        }

        private void btnDiscount_Click(object sender, EventArgs e)
        {

        }
        #endregion




    }
}
