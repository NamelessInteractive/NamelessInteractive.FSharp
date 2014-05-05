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
type RecTest2 = 
    {
        Id: BsonObjectId
        Test1: UnionTest
        Test2: UnionTest
    }

[<TestClass>]
type UnitTest() = 
    let connectionString = "mongodb://localhost"
    let client = new MongoDB.Driver.MongoClient(connectionString)
    let server = client.GetServer()
    let database = server.GetDatabase("TestSerializers")
    let insertRecord = 
        {
            Id = BsonObjectId(ObjectId.GenerateNewId())
            Test1 = Case1(5)
            Test2 = Case2
        }
    do 
        NamelessInteractive.FSharp.MongoDB.SerializationProviderModule.Register()
    

    [<TestMethod>]
    member x.TestMethod1 () = 
        let collection = database.GetCollection<RecTest2>("Records")
        collection.Insert(insertRecord) |> ignore
        Assert.IsTrue(true)
        let result = collection.FindOne(MongoDB.Driver.Builders.Query.EQ("_id",insertRecord.Id))
        Assert.AreEqual(insertRecord,result)
