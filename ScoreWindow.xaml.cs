using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FaceTrackingBasics
{
    /// <summary>
    /// Interaction logic for ScoreWindow.xaml
    /// </summary>
    public partial class ScoreWindow : Window
    {
        public ScoreWindow(List<Game> games)
        {
            InitializeComponent();
            int cols = 4;
            int rows = games.Count();
            for (int c = 0; c < cols; c++)
                myTable.Columns.Add(new TableColumn());

            TableRow tr0 = new TableRow();
            tr0.Cells.Add(new TableCell(new Paragraph(new Run("Gra"))));
            tr0.Cells.Add(new TableCell(new Paragraph(new Run("Minimalny pomiar"))));
            tr0.Cells.Add(new TableCell(new Paragraph(new Run("Maksymalny pomiar"))));
            tr0.Cells.Add(new TableCell(new Paragraph(new Run("Odchylenie standardowe"))));
            TableRowGroup trg0 = new TableRowGroup();
            trg0.Rows.Add(tr0);
            myTable.RowGroups.Add(trg0);

            foreach (Game g in games)
            {
               TableRow tr = new TableRow();
               tr.Cells.Add(new TableCell(new Paragraph(new Run(g.getName()))));
               tr.Cells.Add(new TableCell(new Paragraph(new Run(g.getMinimum().ToString("N3")))));
               tr.Cells.Add(new TableCell(new Paragraph(new Run(g.getMaximum().ToString("N3")))));
               tr.Cells.Add(new TableCell(new Paragraph(new Run(g.getDeviation().ToString("N3")))));

               TableRowGroup trg = new TableRowGroup();
               trg.Rows.Add(tr);
               myTable.RowGroups.Add(trg);
            }
        }
    }
}
