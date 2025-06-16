using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

public class CalendarColumn : DataGridViewColumn
{
    public CalendarColumn() : base(new CalendarCell()) { }

    public override DataGridViewCell CellTemplate
    {
        get { return base.CellTemplate; }
        set
        {
            if (value != null && !value.GetType().IsAssignableFrom(typeof(CalendarCell)))
            {
                throw new InvalidCastException("Deve ser do tipo CalendarCell");
            }
            base.CellTemplate = value;
        }
    }
}

public class CalendarCell : DataGridViewTextBoxCell
{
    public CalendarCell() : base()
    {
        this.Style.Format = "dd/MM/yyyy HH:mm";
    }

    public override void InitializeEditingControl(int rowIndex, object initialFormattedValue, DataGridViewCellStyle dataGridViewCellStyle)
    {
        base.InitializeEditingControl(rowIndex, initialFormattedValue, dataGridViewCellStyle);
        CalendarEditingControl ctl = DataGridView.EditingControl as CalendarEditingControl;
        if (this.Value == null || this.Value == DBNull.Value)
        {
            ctl.Value = DateTime.Now;
        }
        else
        {
            ctl.Value = Convert.ToDateTime(this.Value);
        }
    }

    public override Type EditType => typeof(CalendarEditingControl);

    public override Type ValueType => typeof(DateTime);

    public override object DefaultNewRowValue => DateTime.Now;
}

public class CalendarEditingControl : DateTimePicker, IDataGridViewEditingControl
{
    DataGridView dataGridView;
    private bool valueChanged = false;
    int rowIndex;

    public CalendarEditingControl()
    {
        this.Format = DateTimePickerFormat.Custom;
        this.CustomFormat = "dd/MM/yyyy HH:mm";
        this.ShowUpDown = true;
    }

    public object EditingControlFormattedValue
    {
        get { return this.Value.ToString("dd/MM/yyyy HH:mm"); }
        set
        {
            if (value is string)
            {
                DateTime.TryParse((string)value, out DateTime result);
                this.Value = result;
            }
        }
    }

    public object GetEditingControlFormattedValue(DataGridViewDataErrorContexts context)
    {
        return EditingControlFormattedValue;
    }

    public void ApplyCellStyleToEditingControl(DataGridViewCellStyle dataGridViewCellStyle)
    {
        this.Font = dataGridViewCellStyle.Font;
        this.CalendarForeColor = dataGridViewCellStyle.ForeColor;
        this.CalendarMonthBackground = dataGridViewCellStyle.BackColor;
    }

    public int EditingControlRowIndex
    {
        get { return rowIndex; }
        set { rowIndex = value; }
    }

    public bool EditingControlWantsInputKey(Keys key, bool dataGridViewWantsInputKey)
    {
        return key == Keys.Left || key == Keys.Right || key == Keys.Up || key == Keys.Down
               || key == Keys.Home || key == Keys.End || key == Keys.PageDown || key == Keys.PageUp;
    }

    public void PrepareEditingControlForEdit(bool selectAll) { }

    public bool RepositionEditingControlOnValueChange => false;

    public DataGridView EditingControlDataGridView
    {
        get { return dataGridView; }
        set { dataGridView = value; }
    }

    public bool EditingControlValueChanged
    {
        get { return valueChanged; }
        set { valueChanged = value; }
    }

    public Cursor EditingPanelCursor => base.Cursor;

    protected override void OnValueChanged(EventArgs eventargs)
    {
        valueChanged = true;
        this.EditingControlDataGridView.NotifyCurrentCellDirty(true);
        base.OnValueChanged(eventargs);
    }
}