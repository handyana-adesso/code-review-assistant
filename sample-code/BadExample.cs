// ─────────────────────────────────────────────────────────────
//  DEMO-CODE: bewusst fehlerhaft, zum Einfügen in der Live-Demo.
//  Erwartete Funde von Gemma:
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


// ── Optionales zweites Beispiel (TypeScript), falls Zeit bleibt ──
//
// function calcTotal(items) {
//   let total = 0;
//   for (var i = 0; i <= items.length; i++) {   // off-by-one
//     total += items[i].price * items[i].qty;
//   }
//   return total;
// }
