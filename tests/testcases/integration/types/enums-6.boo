#category FailsOnMono4
"""
Info
"""
class Foo:		
	public static Level = LogLevel.Info
	
enum LogLevel:
	None
	Info
	Error

print(Foo.Level)
		
