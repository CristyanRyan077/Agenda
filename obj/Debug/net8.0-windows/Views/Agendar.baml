   M S B A M L   `   `   `  ����E  AAgendaNovo, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null'clr-namespace:AgendaNovo
AgendaNovo  5   	       AgendaNovo.Agendar   5   	   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   S OHandyControl, Version=3.5.1.0, Culture=neutral, PublicKeyToken=45be8712787a1e5b0hc'https://handyorg.github.io/handycontrol  5   	   !vmclr-namespace:AgendaNovo  5   	   _ [MaterialDesignThemes.Wpf, Version=5.2.1.0, Culture=neutral, PublicKeyToken=df2a72020bd7962aFmaterialDesign1http://materialdesigninxaml.net/winfx/xaml/themes  5   	     � Title$  Agendar��5   	   $	��600��5   	   $	��400��6    � WindowStartupLocation$ CenterScreen=�5   	    � WindowState$ 	Maximized=�5   	   ��5      �� 5   
    �� ImageSource$ /Imagens/images.png��6    j� Stretch$ UniformToFill=�67   .��5      �� $
��Auto=�6   $
��Auto=�66   .��5   
   � .+�5      � $��Center=�6   $
��1280��60   $��10q�6@    � RowDefinitions 5      �� 5      $
��Auto �6$   �� 5      $
��Auto �6$   �� 5      $
��Auto �6$   �� 5      $
��Auto �6$   �� 5      $
��Auto �6$   �� 5      $��* �6$    � ColumnDefinitions 5!      �� 5"      $��3* �6'   �� 5#      $
��1.5* �6'   .+�5'      � $��0��6   $��0��6$    5(      �� 5)      $��* �6+   �� 5*      $��* �6+   .+�5-      $  HandyControl.Controls.ComboBox  -   5        $��
txtCliente��5-   "   $��0��64   $	��305��6D   $��50��6P   '  !HandyControl.Controls.InfoElement   Placeholder$ Cliente��5.       �� 
IsEditable$
 True��5/      	 �� DisplayMemberPath$
	 Nome��50      ���	#88D1D3D8
���   �$��
#heveltica)�53   H   $��24(�6`   $
��Bold"�6n   
���   �
   SelectionTextBrush

 �   ���51      �� *ListaClientes+ �� Text 52      �� *NovoCliente.Nome+54   2   '  !MaterialDesignThemes.Wpf.PackIcon  55      $��0��6.      Kind$ Account=�6>   $��46��56   *   $��46��65   $��Center=�6A   
�������$��	248,0,0,0q�57   =     59      -   5        $��txtTelefone��59   "   $��1��65   $	��305��6E   $��50��6Q   $ Telefone��5:      $
 True��5<      ���	#88D1D3D8
���   �$��
#heveltica)�5=   J   $��24(�6b   $
��Bold"�6p   
������ 

 �   � 5;      �� *NovoCliente.Telefone+ �� Mode$ TwoWay=� �� UpdateSourceTrigger$ PropertyChanged=�5>   8     5?      $	��  1��6.   $ Phone=�6@   $��34��5@   *   $��34��65   $��Right=�5A   *   
�������$��Center=�5B   =   5C      � 5F      $��1��6   $��0��6$   $��0,0,0,0q�64    5G      �� 5H      $��* �6+   �� 5I      $��* �6+   .+�5L        -   5        $��	txtpacote��5L   "   $	��400��63   $��55��6?   $ Pacote��5M      ���	#88D1D3D8
���   �$��
#heveltica)�5O   I   $��24(�6a   $
��Bold"�6o   
���   �

 �   �$
��Left=�5P   N   $��3��6i    5N      �� *NovoAgendamento.Pacote+��6F   �� *PacotesDisponiveis+5R        5T      $��1��6.   $ BoxAdd=�6>   $��34��5U   *   $��34��65   $��Center=�6A   
�������$��59,0,0,0q�5V   =     5X      $��1��6"   $	��200��62   $��55��6>   $��Right=�6J   $ Horário��5Y      ���	#88D1D3D8
���   �$��
#heveltica)�5\   I   $��24(�6a   $
��Bold"�6o   
������ 

 �   ���5Z      �� *HorariosDisponiveis+ �� SelectedItem 5[      �� *NovoAgendamento.Horario+5]   9     5^      $��2��6.   $ ClockOutline=�6>   $��31��5_   *   $��34��65   $��Right=�6A   
�������$��Center=�5`   =   5a      � 5d      $��2��6   $��0��6$   $��0,0,0,0q�64    5e      �� 5f      $��2* �6+   �� 5g      $��1* �6+   �� 5h      $��1* �6+   .+�5k      #  HandyControl.Controls.TextBox  $��0��6!   $	��305��61   $��50��6=   $��0,0,0,0q�6I   $
 Tema��5l      ���	#88D1D3D8
���   �$��
#heveltica)�5n   I   $��24(�6a   $
��Bold"�6o   
���   � �� SelectionTextBrush
 �   ���5m      �� *NovoAgendamento.Tema+.��5o   N   5p        5q      $��0��6.   $ Theatre=�6>   $��34��5r   *   $��34��65   $��Right=�5s   *   
�������$��Center=�5t   =     5v      -   5        $��txtValor��5v   !   $��1��61   $	��140��6A   $��50��6M   $ Valor��5w       �� 
IsReadOnly$
 True��5y      ���	#88D1D3D8
���   �$��
#heveltica)�5z   G   $��24(�6_   $
��Bold"�6m   
������ 
 �   ���5x      �� *NovoAgendamento.Valor+ �� StringFormat$ 	R$ {0:N2}��$ PropertyChanged=�5{   7     5|      $ Cash100=�6.    J� Opacity$	 0.6Q�6=   $��Center=�6K   $��1��6f   $��32��6v   $��32��6�   ���	#FFA6FF9F$��	115,0,0,0q�6�     5~      -   5        $��txtvalorpago��5~   "   $��2��5      $	��150��6-   $��50��69   $��Right=�6E   $
 Pago��5�      ���	#88D1D3D8
���   �$��
#heveltica)�5�   I   $��24(�6a   $
��Bold"�6o   
������ 
 �   ���5~   6   �� *NovoAgendamento.ValorPago+$ PropertyChanged=�5�   9     5�      $ 	CashCycle=�6.   $	 0.6Q�6?   $��Center=�6M   $��2��6h   $��Right=�6x   $��32��6�   $��32��6�   ���	#FFA6FF9F6�   5�      � 5�      $��3��6   $��0��6$    5�      �� 5�      $��2* �6+   �� 5�      $��1* �6+   �� 5�      $��1* �6+   .+�5�      � $��0��6   .+�5�        -   5        $��
txtcrianca��5�   '   $��50��5�      $ Criança��5�      $
 True��5�      $
	 Nome��5�      ���	#88D1D3D8
���   �$��
#heveltica)�5�   @   $��24(�5�      $
��Bold"�6$   
���   �

 �   ���5�      �� *ListaCriancas+ 5�      �� *NovoAgendamento.Crianca.Nome+$ TwoWay=�$ PropertyChanged=�5�        5�      $ BabyFaceOutline=�62   $��24��5�   "   $��24��6-   $��	252,0,0,0q�5�   "   ���	#FF00EDFF$��Center=�5�   9   5�        5�      $��1��6"   $��50��62   $ Gênero��5�      $	��150��66   ���	#88D1D3D8
���   �$
��Left=�5�   <   $��
#heveltica)�5�      $��20(�6*   $
��Bold"�68   
������ 

 �   � 5�      �� * NovoAgendamento.Crianca.Genero+.@�5�      3  -HandyControl.Controls.AutoCompleteTextBoxItem  $��M��65     5�      $��F��65   5�      �� 5�       �� Orientation$ 
Horizontal=�6!   $��2��6:   $
��Left=�6J   .+�5�        $��70��6%   $��50��60   $ Idade��6<   ���	#88D1D3D8
���   �$
��Left=�5�   L   $��
#heveltica)�5�   !   $��18(�69   $
��Bold"�6G   
������ 
 �   ���5�      �� *NovoAgendamento.Crianca.Idade+$ PropertyChanged=�5�   ;   �� 5�      $��90��6#   $��50��6.   ���	#88D1D3D8
���   �$
��Left=�5�   U   $��
#heveltica)�5�   +   $��18(�6C   $
��Bold"�6Q   
������ ��5�   +   �� *UnidadesIdade+ 5�   +   �� *&$NovoAgendamento.Crianca.IdadeUnidade+$ PropertyChanged=�5�      5�      �� 5�      $��Limpar Campos��6   
�������$��40��5�   !   $	��110��5�   !   $��4��5�   !   $
��Left=�5�   !   ��5�   !   �� *LimparCamposCommand+5�   !   �� 5�      $��0��6   $��5��6*   $��1��6;   $��Right=�6K   .+�5�      &   System.Windows.Controls.Calendar  $	��350��6   $	��300��6+    � Language$ pt-br�5�         SelectedDate 5�      �� *DataSelecionada+$ TwoWay=�$ PropertyChanged=� � LayoutTransform 5�      �� 5�       �� ScaleX$	 1.1Q�6-    �� ScaleY$	 1.1Q�6:   5�      5�      �� 5�      $��5��6   $��2��6*   $��0,0,10,0q�6>   .+�5�      �� $$��Limpar Agendamentos Anteriores��6   $	��245��5�      $��40��6*   
��� ���
�������$
��Bold"�5�      $��14(�60   $��10q�5�      ��5�      �� *LimparAnterioresCommand+5�      &   System.Windows.Controls.DataGrid  5�         AutoGenerateColumns$ False��6   $	��200��5�      $
��1000��5�      $   HorizontalScrollBarVisibility$
 Auto=�5�      $
��True��5�      ��5�      �� *ListaAgendamentos+ 5�      �� *ItemSelecionado+$ TwoWay=�   Columns 5�      0  *System.Windows.Controls.DataGridTextColumn  5�      ,	  &System.Windows.Controls.DataGridColumn 	  Header$ Id��61   1
  +System.Windows.Controls.DataGridBoundColumn 
  Binding 6=   �� *Id+  5�      $ Cliente��61    6B   �� *Cliente.Nome+  5�      $ Crianca��61    6B   �� *Crianca.Nome+  5�      $ Genero��61    6A   �� *Crianca.Genero+  5�      $ Idade��61    	  
IsReadOnly$
 True��6@    6R   �� *Crianca.IdadeFormatada+  5�      $ Telefone��61    6C   �� *Cliente.Telefone+  5�      $ Pacote��61    6A   �� *Pacote+  5�      $
 Tema��61    6?   �� *Tema+  5�      $ Valor��61    6@   �� *Valor+$ 	R$ {0:N2}��4  .System.Windows.Controls.DataGridTemplateColumn  5�      $
 Pago��65       CellTemplate  5�   "   �� 5�   &   .R�5�   *   �� $
��Bold"�5�   4   $��13(�6F   $��	Helvetica)�6T   ��5�   4   �� *	ValorPago+$ 	R$ {0:N2}����5�   .   �� 5�   2   ! �� 
TargetType"! ��68   .�5�   6   �� " �� Property" � ��# �� Value
# �  ��6S   $ �� Triggers$ 5�   6   �� 5�   :   % �� Binding% 6F   �� *
EstaPago+& �� Value& True6c   .a�5�   >   �� " � ��
# � � �6[   5�   ;   5�   3   5  +   5  '   5       5     $ Horário��61    6B   �� *	Horario+  5     $
 Data��61    6?   �� *Data+$ 
dd/MM/yyyy��5     �� 5
     $ 
Horizontal=�6!   $��Center=�6:   $��0,0,10,0q�6W   .+�5     �� $��Agendar��6!   $	��180��5     $��50��6)   $
��Bold"�5     $��16(�6/   
��� � �
���������5     �� *AgendarCommand+5  0   �� 5     $��Excluir��6!   $	��180��5     $��50��6)   $
��Bold"�5     $��16(�6/   
���  ��
���������5     �� *ExcluirCommand+5  .   5     5     5     5     5     5     