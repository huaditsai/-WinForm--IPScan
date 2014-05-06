using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPScan
{
    class ListViewHelper
    {
        public ListViewHelper()
        {

        }

        public static void ListView_ColumnClick(object sender, System.Windows.Forms.ColumnClickEventArgs e)
        {
            System.Windows.Forms.ListView lv = sender as System.Windows.Forms.ListView;// 檢查點擊的列是不是現在的排序列
            
            if (e.Column == (lv.ListViewItemSorter as ListViewColumnSorter).SortColumn)
            {
                // 重新設置此列的排序方法.
                if ((lv.ListViewItemSorter as ListViewColumnSorter).Order == System.Windows.Forms.SortOrder.Ascending)
                {
                    (lv.ListViewItemSorter as ListViewColumnSorter).Order = System.Windows.Forms.SortOrder.Descending;
                }
                else
                {
                    (lv.ListViewItemSorter as ListViewColumnSorter).Order = System.Windows.Forms.SortOrder.Ascending;
                }
            }
            else
            {
                // 設置排序列，預設小到大
                (lv.ListViewItemSorter as ListViewColumnSorter).SortColumn = e.Column;
                (lv.ListViewItemSorter as ListViewColumnSorter).Order = System.Windows.Forms.SortOrder.Ascending;
            }
            
            ((System.Windows.Forms.ListView)sender).Sort();// 用新的排序方法對ListView排序
        }
    }

    public class ListViewColumnSorter : System.Collections.IComparer //實作IComparer
    {
        private int ColumnToSort;//指定按照哪個列排序
        private System.Windows.Forms.SortOrder OrderOfSort;//指定排序的方式
        private System.Collections.CaseInsensitiveComparer ObjectCompare;//聲明CaseInsensitiveComparer類對像

        public ListViewColumnSorter()
        {            
            ColumnToSort = 0;// 按第一列排序
            
            OrderOfSort = System.Windows.Forms.SortOrder.None;// 排序方式為不排序

            ObjectCompare = new System.Collections.CaseInsensitiveComparer();
        }

        public int Compare(object x, object y)//x=要比較的第一個,y=第二個, x=y return 0, x>y ->1, x<y -> -1
        {
            int compareResult;
            System.Windows.Forms.ListViewItem listviewX, listviewY;

            // 將比較對像轉換為ListViewItem對像
            listviewX = (System.Windows.Forms.ListViewItem)x;
            listviewY = (System.Windows.Forms.ListViewItem)y;

            string xText = listviewX.SubItems[ColumnToSort].Text;
            string yText = listviewY.SubItems[ColumnToSort].Text;

            int xInt, yInt;

            
            if (IsIP(xText) && IsIP(yText))// 比較IP
            {
                compareResult = CompareIp(xText, yText);
            }
            else if (int.TryParse(xText, out xInt) && int.TryParse(yText, out yInt)) //是否全為數字
            {                
                compareResult = CompareInt(xInt, yInt);//比較數字
            }
            else
            {                
                compareResult = ObjectCompare.Compare(xText, yText);//比較對像
            }

            // 根據上面的比較結果返回正確的比較結果
            if (OrderOfSort == System.Windows.Forms.SortOrder.Ascending)
            {                
                return compareResult;// 因為是正序排序，所以直接返回結果
            }
            else if (OrderOfSort == System.Windows.Forms.SortOrder.Descending)
            {                
                return (-compareResult);// 如果是反序排序，所以要取負值再返回
            }
            else
            {
                return 0;//x=y
            }
        }

        public bool IsIP(String ip)
        {
            return System.Text.RegularExpressions.Regex.Match(ip, @"^(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[0-9])$").Success;
        }

        private int CompareInt(int x, int y)
        {
            if (x > y)
            {
                return 1;
            }
            else if (x < y)
            {
                return -1;
            }
            else
            {
                return 0;
            }
        }

        private int CompareIp(string ipx, string ipy)
        {
            string[] ipxs = ipx.Split('.');
            string[] ipys = ipy.Split('.');

            for (int i = 0; i < 4; i++)
            {
                if (Convert.ToInt32(ipxs[i]) > Convert.ToInt32(ipys[i]))
                {
                    return 1;
                }
                else if (Convert.ToInt32(ipxs[i]) < Convert.ToInt32(ipys[i]))
                {
                    return -1;
                }
                else
                {
                    continue;
                }
            }
            return 0;
        }

        public int SortColumn // 得到或設置按照哪一列排序
        {
            set
            {
                ColumnToSort = value;
            }
            get
            {
                return ColumnToSort;
            }
        }

        public System.Windows.Forms.SortOrder Order // 獲取或設置排序方式
        {
            set
            {
                OrderOfSort = value;
            }
            get
            {
                return OrderOfSort;
            }
        }


    }
}
