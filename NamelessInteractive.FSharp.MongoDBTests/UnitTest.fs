namespace UnitTestProject1

open System
open Microsoft.VisualStudio.TestTools.UnitTesting

open MongoDB.Bson
open MongoDB.Driver

type RecTest = 
        {
            Id: string
            Foo: int
            Bar: string
        }

type UnionTest =
    | TestCase1 of int
    | TestCase2 of string
    | TestCase3 of float
    | TestCase4 of decimal
    | TestCase5 of bool
    | TestCase6 of RecTest
    | TestCase7 of RecTest option
    | TestCase8

type RecTestWithUnion = 
    {
        Id: BsonObjectId
        Foo: int
        Bar1: UnionTest
        Bar2: UnionTest
        Bar3: UnionTest
        Bar4: UnionTest
        Bar5: UnionTest
        Bar6: UnionTest
        Bar7: UnionTest
        Bar8: UnionTest
        Bar9: int list
    }

type SmallTest =    
    {
        Foo: UnionTest
    }

[<AutoOpen>]
module Helpers =
    let NewId() = 
        BsonObjectId(ObjectId.GenerateNewId())

[<TestClass>]
type UnitTest() = 
    let connectionString = "mongodb://localhost"
    let client = new MongoDB.Driver.MongoClient(connectionString)
    let database = client.GetDatabase("TestNamelessFSharpMongo")
    do 
        NamelessInteractive.FSharp.MongoDB.SerializationProviderModule.Register()
        NamelessInteractive.FSharp.MongoDB.Conventions.ConventionsModule.Register()
    

    [<TestMethod>]
    member x.TestMethod1 () =
        let wildcard = FilterDefinition<RecTest>.op_Implicit("{}")
        let testCase = 
            {
                RecTest.Id = "Hi"
                RecTest.Foo = 5
                RecTest.Bar = "Baz"
            }

        let serializer = MongoDB.Bson.Serialization.BsonSerializer.LookupSerializer(testCase.GetType())
        let collection = database.GetCollection<RecTest>("RecTest")
        collection.DeleteManyAsync(wildcard) |> ignore
        collection.InsertOneAsync(testCase).Wait() |> ignore
        let saved = collection.Find(wildcard).FirstAsync().Result
        Assert.AreEqual(testCase,saved)

    [<TestMethod>]
    member x.TestMethod2 () =
        let wildcard = FilterDefinition<RecTestWithUnion>.op_Implicit("{}") 
        let testCase = 
            {
                RecTestWithUnion.Id = NewId()
                RecTestWithUnion.Foo = 0
                RecTestWithUnion.Bar1 = UnionTest.TestCase1(2)
                RecTestWithUnion.Bar2 = UnionTest.TestCase2("BazBar")
                RecTestWithUnion.Bar3 = UnionTest.TestCase3(1.4)
                RecTestWithUnion.Bar4 = UnionTest.TestCase4(3.14M)
                RecTestWithUnion.Bar5 = UnionTest.TestCase5(true)
                RecTestWithUnion.Bar6 = UnionTest.TestCase6({ RecTest.Id = "Moo"; RecTest.Foo = 5; RecTest.Bar = "Baz"})
                RecTestWithUnion.Bar7 = UnionTest.TestCase7 (None)
                RecTestWithUnion.Bar8 = UnionTest.TestCase8 
                RecTestWithUnion.Bar9 = [1;2;3;4;5]
            }
        let serializer = MongoDB.Bson.Serialization.BsonSerializer.LookupSerializer(testCase.GetType())
        let collection = database.GetCollection<RecTestWithUnion>("RecTestWithUnion")
        collection.DeleteManyAsync(wildcard).Wait() |> ignore
        collection.InsertOneAsync(testCase).Wait() |> ignore
        let saved = collection.Find(wildcard).FirstAsync().Result
        Assert.AreEqual(testCase,saved)

    [<TestMethod>]
    member x.TestMethod3 () = 
        let wildcard = FilterDefinition<SmallTest>.op_Implicit("{}") 
        let testCase = 
            {
                SmallTest.Foo = UnionTest.TestCase7 None
            }
        let serializer = MongoDB.Bson.Serialization.BsonSerializer.LookupSerializer(testCase.GetType())
        let collection = database.GetCollection<SmallTest>("RecTestWithUnion")
        collection.DeleteManyAsync(wildcard).Wait() |> ignore
        collection.InsertOneAsync(testCase).Wait() |> ignore
        let saved = collection.Find(wildcard).FirstAsync().Result
        Assert.AreEqual(testCase,saved)



