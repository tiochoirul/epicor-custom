this.output = "0/0";

var ordNum = this.Db.OrderHed.Where(x =>
  x.Company == this.Session.CompanyID
  && x.PONum == this.poNum
).FirstOrDefault();
if (ordNum != null) {
  var lineNum = this.Db.OrderDtl.Where(x =>
    x.Company == this.Session.CompanyID
    && x.OrderNum == ordNum.OrderNum
    && x.POLine == this.poItem
  ).FirstOrDefault();
  
  if (lineNum != null) this.output = ordNum.OrderNum.ToString() + "/" + lineNum.OrderLine.ToString();
}