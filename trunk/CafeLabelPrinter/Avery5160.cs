using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows;
using System.Data;
using System.Windows.Markup;

namespace LabelPrinter
{
    /// <summary>
    /// Ambassador Address Label – Avery 5160
    /// </summary>
    public class Avery5160
    {
        //Constants for Avery Address Label 5160
        private const double PAPER_SIZE_WIDTH = 816; //8.5" x 96
        private const double PAPER_SIZE_HEIGHT = 1056; //11" x 96

        private const double LABEL_WIDTH = 252; //2.625" x 96
        private const double LABEL_HEIGHT = 96; //1" x 96

        private const double SIDE_MARGIN = 18.24; //0.19" x 96
        private const double TOP_MARGIN = 48; //0.5" x 96
        private const double HORIZONTAL_GAP = 12.48; //0.13" x 96

        private const int NUM_ROWS = 10;
        private const int NUM_COLUMNS = 3;
        private const int LABELS_PER_SHEET = NUM_COLUMNS * NUM_ROWS; //3 columns of 10 labels

        private FixedPage CreatePage()
        {
            //Create new page
            FixedPage page = new FixedPage();
            //Set background
            page.Background = Brushes.White;
            //Set page size (Letter size)
            page.Width = PAPER_SIZE_WIDTH;
            page.Height = PAPER_SIZE_HEIGHT;
            return page;
        }

        // If topToBottom is true then layout labels from top/left down columns.
        // If topToButton is false then layout labels from top/left across rows.
        public FixedDocument CreateDocument(OrderCollector orders, bool topToBottom)
        {
            //Create new document
            FixedDocument doc = new FixedDocument();
            //Set page size
            doc.DocumentPaginator.PageSize = new Size(PAPER_SIZE_WIDTH, PAPER_SIZE_HEIGHT);

            //Sort output
            orders.sort();

            //Number of records
            double count = (double)orders.count();

            if (count > 0) {
                string line1 = "";
                string line2 = "";
                string line3 = "";
                string line4 = "";

                AveryLabel label;

                //Determine number of pages to generate
                double pageCount = Math.Ceiling(count / LABELS_PER_SHEET);

                int dataIndex = 0;

                for (int i = 0; i < pageCount; i++) {
                    int currentColumn = 0;
                    int currentRow = 0;
                    //Create page
                    PageContent page = new PageContent();
                    FixedPage fixedPage = this.CreatePage();
                    //Create labels
                    for (int j = 0; j < LABELS_PER_SHEET; j++) {
                        determineRowColumn(topToBottom, j, ref currentRow, ref currentColumn);

                        if (dataIndex < count) {
                            //Get data from Order
                            Order order = orders.elementAt(dataIndex);
                            line1 = order.class_ + "   " + order.firstName_ + " " + order.lastName_;
                            line2 = (order.meal_.Count() > 0) ? ("Meal: " + order.meal_) : "";
                            line3 = (order.drink_.Count() > 0) ? ("Drink: " + order.drink_) : "";
                            line4 = (order.extra_.Count() > 0) ? ("Extra: " + order.extra_) : "";

                            //Create individual label
                            label = new AveryLabel(line1, line2, line3, line4);

                            //Set label location
                            if (currentColumn == 0) {
                                FixedPage.SetLeft(label, SIDE_MARGIN);
                            } else if (currentColumn == 1) {
                                FixedPage.SetLeft(label, SIDE_MARGIN + LABEL_WIDTH + HORIZONTAL_GAP);
                            } else {
                                FixedPage.SetLeft(label, SIDE_MARGIN + LABEL_WIDTH * 2 + HORIZONTAL_GAP * 2);
                            }
                            FixedPage.SetTop(label, TOP_MARGIN + currentRow * LABEL_HEIGHT);

                            //Add label object to page
                            fixedPage.Children.Add(label);

                            dataIndex++;
                        }
                    }

                    //Invoke Measure(), Arrange() and UpdateLayout() for drawing
                    fixedPage.Measure(new Size(PAPER_SIZE_WIDTH, PAPER_SIZE_HEIGHT));
                    fixedPage.Arrange(new Rect(new Point(), new Size(PAPER_SIZE_WIDTH, PAPER_SIZE_HEIGHT)));
                    fixedPage.UpdateLayout();

                    ((IAddChild)page).AddChild(fixedPage);

                    doc.Pages.Add(page);
                }
            }

            return doc;
        }

        private static void determineRowColumn(bool topToBottom, int j, ref int currentRow, ref int currentColumn)
        {
            if (topToBottom) {
                if (j % NUM_ROWS == 0) {
                    currentRow = 0;
                } else {
                    currentRow++;
                }
                currentColumn = 0;
                for (int i = 1; i < NUM_COLUMNS; ++i) {
                    if (j >= (NUM_ROWS * i)) {
                        currentColumn = i;
                    }
                }
            } else {
                if (j != 0) {
                    if (j % NUM_COLUMNS == 0) {
                        currentColumn = 0;
                        currentRow++;
                    } else {
                        currentColumn++;
                    }
                }
            }
        }
    }
}
