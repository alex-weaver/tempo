   M S B A M L   `   `   `  ����  MasterDetailSample#    MasterDetailSample.MainWindow   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	     � Title$"  Todo List Sample Application��5   	   $	��350��6.   $	��525��6;   .��5      ]� ��. .+�5   
   ]� ��. .+�5      � $��Bottom=�6   $��8,0,8,8q�6+   .+�5      �� -   5         ��Name$ 	addButton��5      $	��Add��6,   $��80��6:   $��36��6E   $
��Left=�6Q   �� 5	      -   5        $ deleteButton��5	      $��Delete��6/   $��80��6@   $��36��6K   $��Right=�6W   5
      �� 5      -   5         ��Name$ 
masterList��5      $	��200��6+   $��0,0,0,16q�67   6K   5      � 5   
   -   5         �Name$ 
detailView��5      $��10q�6#   .+�5      �� $��
Item text:��6   $��0,20,0,0q�6*   $��66��6<    �� TextAlignment$ Right=�6G   $
��Left=�6]   $	��Top=�6x   �� 5      -   5         ��Name$ 	textField��5      $��
70,16,10,0q�6)   $��26��6=   $	��Top=�6I   �� 5      $��Done?��6   $��0,66,0,0q�6%   $��66��67   $ Right=�6B   $
��Left=�6X   $	��Top=�6s   �� 5      -   5         ��Name$ isDoneCheckbox��5      $��	69,60,0,0q�6/   $
��Left=�6B   $	��Top=�6]    � LayoutTransform 5      �� 5       �� ScaleX$ 2Q�6%   	 �� ScaleY$	 2Q�60   5      5      5      5      