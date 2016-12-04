## RubyStack

Helper library intendent to help C# developers to effectively communicate with `irb` using DTOs.

## Get started

Create Ruby expression class:

* Define parameters
* Specify params order using order attribute
* Set returning type

```
[RubyExpression ("puts \"Hello {0}\"")]
class HelloExpression : IReturn<HelloResult>
{
    [RubyStack.Order (0)]
		public string Name { get; set; }
}

class HelloResult : IReturnExpression<string>
{
    public string Result { get; set; }
    
    public bool Parse (string s)
    {
        ...
    }
}

```

Run expression and get results:

* Start `irb` engine
* Run instance of expression class

```
var helloExpression = new HelloExpression { Name = "John Doe" };

var engine = new RubyEngine ("/path/to/irb");
var result = await engine.Run<HelloResult> (helloExpression);

```

## Questions, issues, feature requests

Please use [GitHub issues](https://github.com/olegoid/RubyStack/issues).

Authors:
Oleg Demchenko

