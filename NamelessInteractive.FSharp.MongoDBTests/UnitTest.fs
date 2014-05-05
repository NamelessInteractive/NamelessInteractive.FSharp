namespace UnitTestProject1

open System
open Microsoft.VisualStudio.TestTools.UnitTesting

open MongoDB.Bson

[<CLIMutable>]
type RecTest = 
        {
            Id: BsonObjectId
            Name: string
        }

type UnionTest =
    | Case1 of int
    | Case2

[<CLIMutable>]
type Address =
    {
        Address1: string
        Address2: string
        Address3: string
        Address4: string
        PostalCode: string
        Country: string
    }

type RecTest2() = 
    member val Id: BsonObjectId = BsonObjectId(ObjectId.GenerateNewId()) with get, set
    member val Test1: UnionTest = Case2 with get, set
    member val Test2: UnionTest = Case2 with get, set
    member val Opt: ResizeArray<string> option = None with get, set
    member val Bob: ResizeArray<string> option = None with get, set

type Brokerage() =
    /// The Identity of the Brokerage
    member val Id: int = 0 with get, set
    /// The Trading Name of the Brokerage
    member val TradingName: string = null with get, set
    /// Type Of Organisation
    member val OrganisationType: string = null with get, set
    /// The Registered Name of the Brokerage
    member val RegisteredName: string = null with get, set
    /// The Company Registration Number
    member val CompanyRegistrationNumber: string = null with get, set
    /// The Company's VAT Number
    member val VATNumber : string = null with get, set
    /// The Member's ID Numbers (Only required if Organisation Type is Close Corporation)
    member val MemberIDNumbers : string[] = [||] with get, set
    /// The Physical Address
    member val PhysicalAddress:  Address option = None with get, set
    /// The Postal Address
    member val PostalAddress : Address option = None with get, set
    /// The Telephone Number
    member val TelephoneNumber: string = null with get, set
    /// The Fax Number
    member val FaxNumber: string = null with get, set
    /// The Company Email Address
    member val EmailAddress: string = null with get, set
    /// The Created Date
    member val DateCreated: DateTime = DateTime.Now with get, set
    /// The Modified Date
    member val DateModified: DateTime = DateTime.Now with get, set

[<TestClass>]
type UnitTest() = 
    let connectionString = "mongodb://localhost"
    let client = new MongoDB.Driver.MongoClient(connectionString)
    let server = client.GetServer()
    let database = server.GetDatabase("GoldenGate")
    do 
        NamelessInteractive.FSharp.MongoDB.SerializationProviderModule.Register()
    

    [<TestMethod>]
    member x.TestMethod1 () = 
        let collection = database.GetCollection<Brokerage>("Brokerages")
        //collection.Insert(insertRecord) |> ignore
        Assert.IsTrue(true)
        let result = collection.FindOne()
        Assert.IsTrue(true)
