using FSMsCore.Enums;
using FSMsCore.Interfaces;

namespace FSMsCore.Models;


public class Order{
    public int Id { get;set;}

    public string Name {get;set;} = "測試";

    public OrderState CurrentState { get; set; }
    
    // public StateFlow Flow { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
}