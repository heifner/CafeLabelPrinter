using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace LabelPrinter
{
    public class Order
    {
        public string error_ = "";
        public string class_ = "??";
        public string firstName_ = "";
        public string lastName_ = "";
        public string mealPrefix_ = "";
        public string meal_ = "";
        public string drink_ = "";
        public string extra_ = "";
    }

    public class OrderCollector
    {
        // orderMap_ contains only the first order
        private Dictionary<string, Order> orderMap_ = new Dictionary<string, Order>();
        private List<Order> orderList_ = new List<Order>();
        private int numErrors_ = 0;

        public Order addOrder(int numMeals, string clss, string firstName, string lastName, string mealPrefix, string meal)
        {
            Order firstOrder = null;
            string key = createKey(firstName, lastName);
            if (!orderMap_.ContainsKey(key)) {
                firstOrder = internalAddOrder(numMeals, clss, firstName, lastName, mealPrefix, meal, false);
                orderMap_.Add(key, firstOrder);
            } else {
                MessageBox.Show("Found more than one: " + key);
            }
            return firstOrder;
        }

        private Order internalAddOrder(int numMeals, string clss, string firstName, string lastName, string mealPrefix, string meal, bool isError)
        {
            Order firstOrder = null;
            for (int i = 0; i < numMeals; ++i) {
                Order order = new Order();
                if (i == 0) {
                    firstOrder = order;
                }
                orderList_.Add(order);
                order.class_ = clss;
                order.firstName_ = firstName;
                order.lastName_ = lastName;
                order.mealPrefix_ = mealPrefix;
                order.meal_ = meal;
                if (isError) {
                    order.error_ = "ERRZ";
                }
            }
            return firstOrder;
        }

        public void addExtra(string firstName, string lastName, string num, string mealCode, string extra, bool isMeal)
        {
            string key = createKey(firstName, lastName);
            bool found = false;
            Order order;
            if (orderMap_.TryGetValue(key, out order)) {
                if (mealCode.Count() > 0 && order.mealPrefix_.Count() > 0) {
                    if (!order.mealPrefix_.Equals(mealCode, StringComparison.OrdinalIgnoreCase)) {
                        string err = firstName + " " + lastName + " meal codes do not match: " + order.mealPrefix_ + ", " + mealCode;
                        MessageBox.Show(err, "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                        order.meal_ = mealCode + ":" + order.meal_;
                    }
                }
                found = true;
            } else {
                if (numErrors_ == 0) {
                    Order errOrder = internalAddOrder(1, "", "ERROR", "ERROR", "ERR", "Orders in Extra not in Main.", false);
                    errOrder.error_ = "ERR";
                }
                numErrors_++;
            }
            if (isMeal || !found) {
                int numMeals = Convert.ToInt32(num);
                string clss = (found) ? order.class_ : "??";
                bool isError = (found) ? (order.error_.Count() > 0) : true;
                order = internalAddOrder(numMeals, clss, firstName, lastName, mealCode, extra, isError);
                if (!found) {
                    orderMap_.Add(key, order);
                }
                return;
            }
            if (mealCode.Count() > 0) {
                if (order.extra_.Count() != 0) order.extra_ += ", ";
                order.extra_ += num + " " + extra;
            } else {
                if (order.drink_.Count() != 0) order.drink_ += ", ";
                order.drink_ += num + " " + extra;
            }
        }

        private string createKey(string firstName, string lastName)
        {
            return lastName + ", " + firstName;
        }

        public void clear()
        {
            orderMap_.Clear();
            orderList_.Clear();
            numErrors_ = 0;
        }

        public int count()
        {
            return orderList_.Count;
        }

        public void sort()
        {
            orderList_ = orderList_
                .OrderBy(x => x.error_)
                .ThenBy(x => x.class_, new NaturalSorting.NaturalComparer(true))
                .ThenBy(x => x.lastName_)
                .ThenBy(x => x.firstName_)
                .ToList();
        }

        public Order elementAt(int i)
        {
            return orderList_.ElementAt(i);
        }

    }
}
