// ─────────────────────────────────────────────────────────────
//  DEMO-CODE: 
//  Expected found by Gemma:
//   • Sicherheit  → SQL-Injection (String-Konkatenation)
//   • Performance → keine using/Dispose, Verbindung bleibt offen
//   • Best Practice → keine Null-/Existenz-Prüfung, kein async,
//                     "SELECT *", kein Parametrisieren, kein Logging
// ─────────────────────────────────────────────────────────────

public string GetUser(string id)
{
    var conn = new SqlConnection(connStr);
    conn.Open();
    var cmd = new SqlCommand("SELECT * FROM Users WHERE Id = '" + id + "'", conn);
    var reader = cmd.ExecuteReader();
    reader.Read();
    var name = reader["Name"].ToString();
    return name;
}

// ── Optionales zweites Beispiel

public async Task<decimal> GetOrderTotalAsync(int orderId)
{
    var order = _db.Orders.FirstOrDefault(o => o.Id == orderId);

    decimal total = 0;
    foreach (var item in order.Items)
    {
        total += item.Price * item.Quantity;
    }

    if (order.Customer.IsVip = true)
        total = total - total * 0.1m;

    var log = new StreamWriter("C:\\logs\\orders.txt", true);
    log.WriteLine($"Order {orderId} total: {total}");

    return total;
}


// ── Optionales drittes Beispiel (TypeScript) ──
//
// function calcTotal(items) {
//   let total = 0;
//   for (var i = 0; i <= items.length; i++) {   // off-by-one
//     total += items[i].price * items[i].qty;
//   }
//   return total;
// }
