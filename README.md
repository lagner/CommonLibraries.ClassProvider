ClassProvider
===
Microsoft Silverlight has a great and very halpfull control - DataGrid. It's indispensable when you need to show a lot of data. But it has a nuance. DataGrid works with objects and binds only with public properties.  
The ClassProvider was developed to solve the problem. If you have a collection of columns you can use ClassProvider to create new Type at runtime. This Type will contain nessessary public properties and support INotifyPropertyChanged interface. 