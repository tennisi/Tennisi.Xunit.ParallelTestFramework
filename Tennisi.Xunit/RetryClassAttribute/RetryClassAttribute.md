# RetryClassAttribute constructor

Initializes a new instance of the [`RetryClassAttribute`](../RetryClassAttribute.md) class with the specified retry count.

```csharp
public RetryClassAttribute(int retryCount)
```

| parameter | description |
| --- | --- |
| retryCount | The number of retry attempts for test methods in the class. Must be greater than one. |

## Exceptions

| exception | condition |
| --- | --- |
| ArgumentOutOfRangeException | Thrown when *retryCount* is less than or equal to one. |

## See Also

* class [RetryClassAttribute](../RetryClassAttribute.md)
* namespace [Tennisi.Xunit](../../Tennisi.Xunit.v2.ParallelTestFramework.md)

<!-- DO NOT EDIT: generated by xmldocmd for Tennisi.Xunit.v2.ParallelTestFramework.dll -->
