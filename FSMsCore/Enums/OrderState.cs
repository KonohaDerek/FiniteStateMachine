using FSMsCore.Interfaces;

namespace FSMsCore.Enums;

public enum OrderState
{
    Done = -1,
    New ,
    Padding,
    PaySuccess,
    PayFailed,
    Completed,
    Faild,
    Canceled,
    Expired,

}