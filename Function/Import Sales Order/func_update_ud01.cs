try{
  using(var txScope = Erp.ErpContext.CreateDefaultTransactionScope()){
    CallService<Ice.Contracts.UD01SvcContract>(svc => {
     var dataUD01 = Db.UD01.Where(x => x.Key2 == this.PONum && x.Date01 == this.Date && x.Character01 == this.PlantAHM && x.ShortChar04 == this.POItem && x.Character02 == this.PartNum).FirstOrDefault();
     if (dataUD01 != null) {
       var ds = new Ice.Tablesets.UD01Tableset();
       ds = svc.GetByID(dataUD01.Key1, dataUD01.Key2, dataUD01.Key3, dataUD01.Key4, dataUD01.Key5);
       ds.UD01[0].CheckBox01 = true;
       ds.UD01[0].RowMod = "U";
       svc.Update(ref ds);
     }
    });
    txScope.Complete();
  }
} catch(Exception e){
  throw new Ice.BLException($"Update UD01: {e.Message}");
}
