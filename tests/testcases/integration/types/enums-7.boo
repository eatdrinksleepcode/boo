#category FailsOnMono4
"""
Info
"""
class Foo:
	
	enum LogLevel:
		None
		Info
		Error

	public static Level = LogLevel.Info
	

print(Foo.Level)
		
