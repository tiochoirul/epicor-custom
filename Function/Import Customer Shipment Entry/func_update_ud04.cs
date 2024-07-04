try{
  using(var txScope = Erp.ErpContext.CreateDefaultTransactionScope()){
    CallService<Ice.Contracts.UD04SvcContract>(svc => {
     var dataUD04 = Db.UD04.Where(x => x.Key1 == this.Key1).FirstOrDefault();
     if (dataUD04 != null) {
       var ds = new Ice.Tablesets.UD04Tableset();
       ds = svc.GetByID(dataUD04.Key1, dataUD04.Key2, dataUD04.Key3, dataUD04.Key4, dataUD04.Key5);
       ds.UD04[0].CheckBox01 = true;
       ds.UD04[0].RowMod = "U";
       svc.Update(ref ds);
     }
    });
    txScope.Complete();
  }
} catch(Exception e){
  throw new Ice.BLException($"Update UD04: {e.Message}");
}
