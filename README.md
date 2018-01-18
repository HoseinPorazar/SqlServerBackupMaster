

Automatic Backup Multiple SQL Servers Databases
-With progressbar to show process 
-Used Thread for backup process to prevent form freez


We have more than 1 SQL Server instances in our company and we take backups of  databases every day
so i made this application to backup databases automatically
(I have Scheduled a task in my windows to start my application every morning and backup databases and exit app after finishing )
-To setup application i  first  add servers  in AddServer form , Username and passwords will be saved in application folder for next time
i have used a simple base64 encoding to secure username and password(you can use more stronger methods to secure passwords)
after adding servers, application will list all of databases within that server inside a treeview control (each server displayed with nodes and each database in child nodes)
Selected databases will appear in the second treeview at right side (selected databases info will be saved in application folder in xml format)

Pressing backup button will start backup process and backup selected databases one by one 


