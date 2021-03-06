"""
Exception with type 'System.ArgumentOutOfRangeException' handled by System.ArgumentException handler
"""

def ExceptionCaught():
	ExceptionCaught(null, null)
def ExceptionCaught(type as System.Type):
	ExceptionCaught(null, type)
def ExceptionCaught(ex as object):
	ExceptionCaught(ex, null)
def ExceptionCaught(ex as object, exHandler as System.Type):
	value as string
	value += "Anonymous " if ex is null
	value += "Exception "
	value += "with type '${ex.GetType()}' " if ex is not null
	value += "handled by "
	value += "${exHandler} handler" if exHandler is not null
	value += "default handler" if exHandler is null
	print value

try:
	raise System.ArgumentOutOfRangeException()
except ex as System.ArgumentNullException:
	ExceptionCaught(ex, System.ArgumentNullException)
except ex as System.ArgumentException:
	ExceptionCaught(ex, System.ArgumentException)
except ex:
	ExceptionCaught(ex)
