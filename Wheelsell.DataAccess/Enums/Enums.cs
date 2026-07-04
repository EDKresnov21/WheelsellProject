namespace Wheelsell.DataAccess.Enums;

public enum UserRole
{
    User = 0,
    Moderator = 1,
    Admin = 2
}

public enum AdvertStatus
{
    Active = 0,
    Sold = 1,
    OffSale = 2
}

public enum FuelType
{
    Petrol = 0,
    Diesel = 1,
    Electric = 2,
    Hybrid = 3,
    PlugInHybrid = 4,
    LPG = 5,
    CNG = 6,
    Hydrogen = 7
}

public enum TransmissionType
{
    Manual = 0,
    Automatic = 1,
    SemiAutomatic = 2
}

public enum BodyType
{
    Sedan = 0,
    Hatchback = 1,
    Suv = 2,
    Coupe = 3,
    Convertible = 4,
    Wagon = 5,
    Pickup = 6,
    Van = 7,
    Minivan = 8,
    Crossover = 9
}

public enum DrivetrainType
{
    FrontWheelDrive = 0,
    RearWheelDrive = 1,
    AllWheelDrive = 2,
    FourByFour = 3
}

public enum CarCondition
{
    New = 0,
    Repaired = 1,
    Damaged = 2,
    Crashed = 3,
    ForParts = 4
}

public enum NotificationType
{
    NewMessage = 0,
    AdvertSold = 1,
    NewReview = 2,
    AdvertBanned = 3,
    AccountBanned = 4,
    EmailConfirmed = 5
}
