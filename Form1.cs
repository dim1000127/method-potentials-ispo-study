using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MethodPotencialov
{
    public partial class Form1 : Form
    {
        struct Element
        {
            public int valueCost;
            public bool check;
        }

        struct U
        {
            public int valueU;
            public bool checkU;
            public bool visible;
        }

        struct V
        {
            public int valueV;
            public bool checkV;
            public bool visible;
        }

        private Element[,] coefDelivery;
        private int[,] IndirectTariffs;
        private int[,] Estimate;

        private U[] StocksU;
        private V[] NeedsV;

        public Form1()
        {
            InitializeComponent();
            dataGridView2.RowCount = 1;
            dataGridView2.Columns[0].Width = 60;
            buttonCalculate.Enabled = false;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void numericUpDownRow_ValueChanged(object sender, EventArgs e)
        {
            AddGridRowCol();
        }

        private void numericUpDownCol_ValueChanged(object sender, EventArgs e)
        {
            AddGridRowCol();
        }

        private void AddGridRowCol()
        {

            dataGridView2.RowCount = (int)numericUpDownRow.Value + 1;
            dataGridView2.ColumnCount = (int)numericUpDownCol.Value + 1;

            dataGridView2.Rows[0].DefaultCellStyle.BackColor = Color.Gray;
            dataGridView2.Columns[0].DefaultCellStyle.BackColor = Color.Gray;

            for (int j = 1; j < (dataGridView2.RowCount); j++)
            {
                for (int i = 1; i < dataGridView2.ColumnCount; i++)
                {
                    dataGridView2.Columns[i].HeaderText = string.Format("b" + i.ToString());
                    dataGridView2.Rows[j].HeaderCell.Value = string.Format("a" + (j).ToString());
                }
            }

            foreach (DataGridViewColumn column in dataGridView2.Columns)
                column.Width = 60;
        }

        private void dataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            var colorVal = dataGridView2.Rows[e.RowIndex].Cells[e.ColumnIndex].Style.BackColor;
            if (colorVal != Color.GreenYellow)
            {
                dataGridView2.Rows[e.RowIndex].Cells[e.ColumnIndex].Style.BackColor = Color.GreenYellow;
                coefDelivery[e.RowIndex, e.ColumnIndex].check = true;
            }
            else 
            {
                dataGridView2.Rows[e.RowIndex].Cells[e.ColumnIndex].Style.BackColor = Color.White;
                coefDelivery[e.RowIndex, e.ColumnIndex].check = false;
            }
        }

        private void buttonCalculate_Click(object sender, EventArgs e)
        {
            StocksU = new U[dataGridView2.RowCount];
            NeedsV = new V[dataGridView2.ColumnCount];

            listBoxSteps.Items.Add("Потенциалы");
            while (AllFalse())
            {
                FindUV(StocksU, NeedsV);
            }
            listBoxSteps.Items.Add(" ");
            listBoxSteps.Items.Add("Косвенные тарифы");
            FindIndirectTariffs();
            listBoxSteps.Items.Add(" ");
            listBoxSteps.Items.Add("Оценки");
            FindEstimate();
        }

        private bool AllFalse() 
        {
            bool checkAll = false;

            for (int a = 1; a < (StocksU.Length); a++) 
            {
                if (!StocksU[a].checkU) 
                {
                    checkAll = true;
                }
            }

            for (int b = 1; b < (NeedsV.Length); b++)
            {
                if (!NeedsV[b].checkV)
                {
                    checkAll = true;
                }
            }

            return checkAll;
        }

        private void FindEstimate() 
        {
            int count = 0;

            Estimate = new int[dataGridView2.RowCount, dataGridView2.ColumnCount];

            for (int a = 1; a < (dataGridView2.RowCount); a++)
            {
                for (int b = 1; b < dataGridView2.ColumnCount; b++)
                {
                    if (!coefDelivery[a, b].check)
                    {
                        Estimate[a, b] = coefDelivery[a, b].valueCost - IndirectTariffs[a, b];
                        listBoxSteps.Items.Add($"Оценка ^'[{a}][{b}] = {coefDelivery[a, b].valueCost} - {IndirectTariffs[a, b]} = " + Estimate[a, b]);
                    }
                }
            }

            for (int i = 1; i < Estimate.GetLength(0); i++) 
            {
                for (int j = 1; j < Estimate.GetLength(1); j++) 
                {
                    if (Estimate[i,j] < 0) 
                    {
                        count++;
                    }
                }
            }

            if (count > 0)
            {
                listBoxSteps.Items.Add("НЕОБХОДИМО ПРОДОЛЖИТЬ ПОИСК");
                MessageBox.Show("НЕОБХОДИМО ПРОДОЛЖИТЬ ПОИСК");
            }
            else 
            {
                listBoxSteps.Items.Add("ПОЛУЧЕН ОПТИМАЛЬНЫЙ ПЛАН");
                MessageBox.Show("ПОЛУЧЕН ОПТИМАЛЬНЫЙ ПЛАН");
            }
        }

        private void FindIndirectTariffs()
        {
            IndirectTariffs = new int[dataGridView2.RowCount, dataGridView2.ColumnCount];

            for (int a = 1; a < (dataGridView2.RowCount); a++)
            {
                for (int b = 1; b < dataGridView2.ColumnCount; b++)
                {
                    if (!coefDelivery[a, b].check)
                    {
                        IndirectTariffs[a, b] = StocksU[a].valueU + NeedsV[b].valueV;
                        listBoxSteps.Items.Add($"Косвенный тариф C'[{a}][{b}] = {StocksU[a].valueU} + {NeedsV[b].valueV} = {IndirectTariffs[a, b]}");
                    }
                }
            }
        }

        private void FindUV(U[] StocksU, V[] NeedsV)
        {
            for (int a = 1; a < (dataGridView2.RowCount); a++)
            {
                for (int b = 1; b < dataGridView2.ColumnCount; b++)
                {
                    if (coefDelivery[a, b].check && !NeedsV[b].checkV && !StocksU[a].checkU) 
                    {
                        if (!CheckcoefDeliveryNext())
                        {
                            StocksU[a].valueU = 0;
                            StocksU[a].checkU = true;
                            if (!StocksU[a].visible)
                            {
                                listBoxSteps.Items.Add($"Потенциал U{a} = {StocksU[a].valueU}");
                                StocksU[a].visible = true;
                            }
                        }
                    }
                    
                    if (coefDelivery[a, b].check && !NeedsV[b].checkV) 
                    {
                        if (StocksU[a].checkU)
                        {
                            NeedsV[b].valueV = coefDelivery[a, b].valueCost - StocksU[a].valueU;
                            NeedsV[b].checkV = true;
                            if (!NeedsV[b].visible)
                            {
                                listBoxSteps.Items.Add($"Потенциал V{b} = {NeedsV[b].valueV}");
                                NeedsV[b].visible = true;
                            }
                            continue;
                        }
                    }

                    if (coefDelivery[a, b].check && NeedsV[b].checkV) 
                    {
                        StocksU[a].valueU = coefDelivery[a, b].valueCost - NeedsV[b].valueV;
                        StocksU[a].checkU = true;
                        if (!StocksU[a].visible)
                        {
                            listBoxSteps.Items.Add($"Потенциал U{a} = {StocksU[a].valueU}");
                            StocksU[a].visible = true;
                        }
                        continue;
                    }
                }
            }
        }

        private bool CheckcoefDeliveryNext() 
        {
            int count = 0;

            for (int a = 1; a < (dataGridView2.RowCount); a++)
            {
                for (int b = 1; b < dataGridView2.ColumnCount; b++)
                {
                    if (coefDelivery[a, b].check && (StocksU[a].checkU || NeedsV[b].checkV))
                    {
                        if (StocksU[a].checkU) 
                        {
                            NeedsV[b].valueV = coefDelivery[a, b].valueCost - StocksU[a].valueU;
                            NeedsV[b].checkV = true;
                            if (!NeedsV[b].visible) 
                            {
                                listBoxSteps.Items.Add($"Потенциал V{b} = {NeedsV[b].valueV}");
                                NeedsV[b].visible = true;
                                count++;
                            }
                        }

                        if (NeedsV[b].checkV)
                        {
                            StocksU[a].valueU = coefDelivery[a, b].valueCost - NeedsV[b].valueV;
                            StocksU[a].checkU = true;
                            if (!StocksU[a].visible) 
                            {
                                listBoxSteps.Items.Add($"Потенциал U{a} = {StocksU[a].valueU}");
                                StocksU[a].visible = true;
                                count++;
                            }
                        }
                    }
                }
            }

            if (count >= 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void createMatrix()
        {
            coefDelivery = null;

            coefDelivery = new Element[dataGridView2.RowCount, dataGridView2.ColumnCount];

            for (int i = 1; i < dataGridView2.RowCount; i++)
            {
                for (int j = 1; j < dataGridView2.ColumnCount; j++)
                {
                    coefDelivery[i, j].valueCost = Convert.ToInt32(dataGridView2.Rows[i].Cells[j].Value);
                    var colorVal = dataGridView2.Rows[i].Cells[j].Style.BackColor;
                    if (colorVal == Color.GreenYellow)
                    {
                        coefDelivery[i, j].check = true;
                    }
                }
            }
        }

        private void buttonCreateMas_Click(object sender, EventArgs e)
        {
            createMatrix();
            buttonCalculate.Enabled = true;
        }
    }
}
