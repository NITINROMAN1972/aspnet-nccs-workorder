using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.Util;

public partial class Work_Order_WorkOrder : System.Web.UI.Page
{
    string connectionString = ConfigurationManager.ConnectionStrings["Ginie"].ConnectionString;
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            Search_DD_RequistionNo();

            //Session["UserId"] = "10024"; // milind - client
        }
    }


    //=========================={ Paging & Alert }==========================
    protected void gridSearch_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        //binding GridView to PageIndex object
        gridSearch.PageIndex = e.NewPageIndex;

        DataTable pagination = (DataTable)Session["PaginationDataSource"];

        gridSearch.DataSource = pagination;
        gridSearch.DataBind();
    }

    private void alert(string mssg)
    {
        // alert pop - up with only message
        string message = mssg;
        string script = $"alert('{message}');";
        ScriptManager.RegisterStartupScript(this, this.GetType(), "messageScript", script, true);
    }



    //=========================={ Sweet Alert JS }==========================

    private void getSweetAlertWarningMandatory(string titles, string mssg)
    {
        string title = titles;
        string message = mssg;
        string icon = "warning";
        string confirmButtonText = "OK";
        string allowOutsideClick = "false"; // Prevent closing on outside click

        string sweetAlertScript =
        $@"<script>
            Swal.fire({{ 
                title: '{title}', 
                text: '{message}', 
                icon: '{icon}', 
                confirmButtonText: '{confirmButtonText}', 
                allowOutsideClick: {allowOutsideClick}
            }});
        </script>";
        ClientScript.RegisterStartupScript(this.GetType(), "sweetAlert", sweetAlertScript, false);
    }

    private void getSweetAlertSuccessRedirectMandatory(string titles, string mssg, string redirectUrl)
    {
        string title = titles;
        string message = mssg;
        string icon = "success";
        string confirmButtonText = "OK";
        string allowOutsideClick = "false"; // Prevent closing on outside click

        string sweetAlertScript =
        $@"<script>
            Swal.fire({{ 
                title: '{title}', 
                text: '{message}', 
                icon: '{icon}', 
                confirmButtonText: '{confirmButtonText}', 
                allowOutsideClick: {allowOutsideClick}
            }}).then((result) => {{
                if (result.isConfirmed) {{
                    window.location.href = '{redirectUrl}';
                }}
            }});
        </script>";
        ClientScript.RegisterStartupScript(this.GetType(), "sweetAlert", sweetAlertScript, false);
    }

    // sweet alert - error only block
    private void getSweetAlertErrorMandatory(string titles, string mssg)
    {
        string title = titles;
        string message = mssg;
        string icon = "error";
        string confirmButtonText = "OK";
        string allowOutsideClick = "false"; // Prevent closing on outside click

        string sweetAlertScript =
        $@"<script>
            Swal.fire({{ 
                title: '{title}', 
                text: '{message}', 
                icon: '{icon}', 
                confirmButtonText: '{confirmButtonText}', 
                allowOutsideClick: {allowOutsideClick}
            }});
        </script>";
        ClientScript.RegisterStartupScript(this.GetType(), "sweetAlert", sweetAlertScript, false);
    }

    // info
    private void getSweetAlertInfoMandatory(string titles, string mssg)
    {
        string title = titles;
        string message = mssg;
        string icon = "info";
        string confirmButtonText = "OK";
        string allowOutsideClick = "false"; // Prevent closing on outside click

        string sweetAlertScript =
        $@"<script>
            Swal.fire({{ 
                title: '{title}', 
                text: '{message}', 
                icon: '{icon}', 
                confirmButtonText: '{confirmButtonText}', 
                allowOutsideClick: {allowOutsideClick}
            }});
        </script>";
        ClientScript.RegisterStartupScript(this.GetType(), "sweetAlert", sweetAlertScript, false);
    }


    //=========================={ Binding Search Dropdowns }==========================
    private void Search_DD_RequistionNo()
    {
        using (SqlConnection con = new SqlConnection(connectionString))
        {
            con.Open();
            string sql = $@"select * 
                            from PaymentStatus891 as pay 
                            inner join Requisition1891 as req1 on pay.Req1RefNo = req1.RefNo";

            SqlCommand cmd = new SqlCommand(sql, con);
            cmd.ExecuteNonQuery();

            SqlDataAdapter ad = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            ad.Fill(dt);
            con.Close();

            ddScRequisitionNo.DataSource = dt;
            ddScRequisitionNo.DataTextField = "ReqNo";
            ddScRequisitionNo.DataValueField = "ReqNo";
            ddScRequisitionNo.DataBind();
            ddScRequisitionNo.Items.Insert(0, new ListItem("------Select Requisition No------", "0"));
        }
    }




    //=========================={ Drop Down Event }==========================
    protected void TDSSection_SelectedIndexChanged(object sender, EventArgs e)
    {
        string tdsSectionRefID = TDSSection.SelectedValue;

        if (TDSSection.SelectedValue != "0")
        {
            CalculateTDS(tdsSectionRefID);
        }
        else
        {
            PaymentNature.Value = string.Empty;
            TDSRate.Text = string.Empty;

            double netAmount = Convert.ToDouble(txtNetAmnt.Text);
            PaymentAmount.Text = Math.Floor(netAmount).ToString("");
            RightOffAmount.Text = (netAmount % 1).ToString("N2");
            TDSAmount.Text = (0).ToString("");

            TDSAmount.Text = (0).ToString();

            // clearing the items in the AgriCollg dropdown
            //AgriCollg.Items.Clear();
            //AgriCollg.Items.Insert(0, new ListItem("------Select Aggriculture College------", "0"));
        }
    }

    private void CalculateTDS(string tdsSectionRefID)
    {
        // autp fill payment nature & tax rate (%)
        using (SqlConnection con = new SqlConnection(connectionString))
        {
            con.Open();
            string sql = "select * from TdsSection891 where RefID = @RefID";
            SqlCommand cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@RefID", tdsSectionRefID);
            cmd.ExecuteNonQuery();

            SqlDataAdapter ad = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            ad.Fill(dt);
            con.Close();

            if (dt.Rows.Count > 0)
            {
                tdsAmountTextbox.Visible = true;

                string paymentNature = dt.Rows[0]["PayNature"].ToString();
                double tdsRate = Convert.ToDouble(dt.Rows[0]["TdsRate"]);

                PaymentNature.Value = paymentNature;
                TDSRate.Text = tdsRate.ToString();

                // deducting tds rate from netamount amount
                double netAmount = Convert.ToDouble(txtNetAmnt.Text);

                double tdsAmount = Convert.ToDouble((netAmount * (tdsRate / 100)));

                double paymentAmount = netAmount - tdsAmount;

                // new tds deducted payment amount
                TDSAmount.Text = tdsAmount.ToString("");
                PaymentAmount.Text = Math.Floor(paymentAmount).ToString("");
                RightOffAmount.Text = (paymentAmount % 1).ToString("N2");
            }
            else
            {
                double netAmount = Convert.ToDouble(txtNetAmnt.Text);
                TDSAmount.Text = (0).ToString("");
                PaymentAmount.Text = Math.Floor(netAmount).ToString();
                RightOffAmount.Text = (netAmount % 1).ToString("N2");

                // emptying tds section
                TDSSection.SelectedIndex = 0;
                PaymentNature.Value = string.Empty;
                TDSRate.Text = (0).ToString();
            }
        }
    }





    //=========================={ Fetch Data }==========================

    private DataTable GetRequisitionDT(string ReqNo)
    {
        using (SqlConnection con = new SqlConnection(connectionString))
        {
            con.Open();
            string sql = "select * from Requisition1891 where ReqNo = @ReqNo";
            SqlCommand cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@ReqNo", ReqNo);
            cmd.ExecuteNonQuery();

            SqlDataAdapter ad = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            ad.Fill(dt);
            con.Close();

            return dt;
        }
    }

    private string GetNewPaymentRefNo(SqlConnection con, SqlTransaction transaction)
    {
        string nextRefNo = "1000001";

        string sql = "SELECT ISNULL(MAX(CAST(RefNo AS INT)), 1000000) + 1 AS NextRefNo FROM PaymentStatus891";
        SqlCommand cmd = new SqlCommand(sql, con, transaction);

        SqlDataAdapter ad = new SqlDataAdapter(cmd);
        DataTable dt = new DataTable();
        ad.Fill(dt);

        if (dt.Rows.Count > 0) return dt.Rows[0]["NextRefNo"].ToString();
        else return nextRefNo;
    }

    private DataTable getAccountHeadExisting(string req1RefNo)
    {
        using (SqlConnection con = new SqlConnection(connectionString))
        {
            con.Open();
            string sql = "select * from ReqTaxHead891 where Req1RefNo = @Req1RefNo";
            SqlCommand cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@Req1RefNo", req1RefNo);
            cmd.ExecuteNonQuery();

            SqlDataAdapter ad = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            ad.Fill(dt);
            con.Close();

            return dt;
        }
    }

    private bool checkForDocuUploadedExist(string docRefNo, SqlConnection con, SqlTransaction transaction)
    {
        string sql = "SELECT * FROM BillDocUpload891 WHERE RefNo=@RefNo";

        SqlCommand cmd = new SqlCommand(sql, con, transaction);
        cmd.Parameters.AddWithValue("@RefNo", docRefNo);
        cmd.ExecuteNonQuery();

        SqlDataAdapter ad = new SqlDataAdapter(cmd);
        DataTable dt = new DataTable();
        ad.Fill(dt);

        if (dt.Rows.Count > 0) return true;
        else return false;
    }

    private string GetNewDocReferenceNo(SqlConnection con, SqlTransaction transaction)
    {
        string nextRefNo = "1000001";

        string sql = "SELECT ISNULL(MAX(CAST(RefNo AS INT)), 1000000) + 1 AS NextRefNo FROM BillDocUpload891";
        SqlCommand cmd = new SqlCommand(sql, con, transaction);

        SqlDataAdapter ad = new SqlDataAdapter(cmd);
        DataTable dt = new DataTable();
        ad.Fill(dt);

        if (dt.Rows.Count > 0) nextRefNo = dt.Rows[0]["NextRefNo"].ToString();
        return nextRefNo;
    }

    private string GetNewWorkOrderRefNo(SqlConnection con, SqlTransaction transaction)
    {
        string nextRefNo = "1000001";

        string sql = "SELECT ISNULL(MAX(CAST(RefNo AS INT)), 1000000) + 1 AS NextRefNo FROM ReqWorkOrder891";
        SqlCommand cmd = new SqlCommand(sql, con, transaction);

        SqlDataAdapter ad = new SqlDataAdapter(cmd);
        DataTable dt = new DataTable();
        ad.Fill(dt);

        if (dt.Rows.Count > 0) nextRefNo = dt.Rows[0]["NextRefNo"].ToString();
        return nextRefNo;
    }

    private DataTable CheckForExistingBillingStatus(string req1RefNo)
    {
        using (SqlConnection con = new SqlConnection(connectionString))
        {
            con.Open();
            string sql = $@"select * from ReqWorkOrder891 where Req1RefNo = @Req1RefNo";

            SqlCommand cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@Req1RefNo", req1RefNo);
            cmd.ExecuteNonQuery();

            SqlDataAdapter ad = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            ad.Fill(dt);
            con.Close();

            return dt;
        }
    }



    //=========================={ Search Button Event }==========================
    protected void btnNewBill_Click(object sender, EventArgs e)
    {
        Response.Redirect("RequisitionReceived.aspx");
    }

    protected void btnSearch_Click(object sender, EventArgs e)
    {
        BindGridView();
    }

    private void BindGridView()
    {
        searchGridDiv.Visible = true;

        // dropdown values
        string reqNo = ddScRequisitionNo.SelectedValue;

        DateTime fromDate;
        DateTime toDate;

        if (!DateTime.TryParse(ScFromDate.Text, out fromDate)) { fromDate = SqlDateTime.MinValue.Value; }
        if (!DateTime.TryParse(ScToDate.Text, out toDate)) { toDate = SqlDateTime.MaxValue.Value; }

        // DTs
        DataTable reqDT = GetRequisitionDT(reqNo);

        // dt values
        string requisitionNo = (reqDT.Rows.Count > 0) ? reqDT.Rows[0]["ReqNo"].ToString() : string.Empty;

        DataTable searchResultDT = SearchRecords(requisitionNo, fromDate, toDate);

        if (searchResultDT.Rows.Count > 0)
        {
            // binding the search grid
            gridSearch.DataSource = searchResultDT;
            gridSearch.DataBind();

            Session["PaginationDataSource"] = searchResultDT;
        }
        else
        {
            getSweetAlertWarningMandatory("Payment Status Pending", $"No Payment Has Been Done For Requisition No: {reqNo}");
        }
    }

    public DataTable SearchRecords(string req1RefNo, DateTime fromDate, DateTime toDate)
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            connection.Open();

            string userID = Session["UserId"].ToString();

            string sqlxx = $@"select Distinct pay.*, req1.ReqNo, um.OrgType, um.InstiName, org.OrgTyp, 
                            (select count(*) from requisition2891 as req2 where req2.BillRefNo = req1.RefNo) as Req2Count, 
                            case when wo.Req1RefNo is not null then 'Work Order Issued' else 'Work Order Not Issued' end as WorkOrderStatus 
                            from PaymentStatus891 as pay 
                            inner join Requisition1891 as req1 on pay.Req1RefNo = req1.RefNo 
                            inner join requisition2891 as req2 on req2.BillRefNo = req1.RefNo 
                            INNER JOIN UserMaster891 as um ON req1.SaveBy = um.UserID 
                            INNER JOIN OrgType891 as org ON um.OrgType = org.RefID 
                            left join reqworkorder891 as wo on wo.Req1RefNo = req1.RefNo 
                            WHERE 1=1";

            string sql = $@"select Distinct req1.RefNo as Req1RefNo, req1.ReqNo, um.OrgType, um.InstiName, org.OrgTyp, 
                            pay.RefNo, pay.TransactionNo, pay.TransactionDate, pay.TransactionAmount, 
                            (select count(*) from requisition2891 as req2 where req2.BillRefNo = req1.RefNo) as Req2Count, 
                            case when wo.Req1RefNo is not null then 'Work Order Issued' else 'Work Order Not Issued' end as WorkOrderStatus 
                            from PaymentStatus891 as pay 
                            inner join Requisition1891 as req1 on pay.Req1RefNo = req1.RefNo 
                            inner join requisition2891 as req2 on req2.BillRefNo = req1.RefNo 
                            INNER JOIN UserMaster891 as um ON req1.SaveBy = um.UserID 
                            INNER JOIN OrgType891 as org ON um.OrgType = org.RefID 
                            left join reqworkorder891 as wo on wo.Req1RefNo = req1.RefNo 
                            WHERE 1=1";

            if (!string.IsNullOrEmpty(req1RefNo))
            {
                sql += " AND req1.ReqNo = @ReqNo";
            }

            if (fromDate != null)
            {
                sql += " AND pay.TransactionDate >= @FromDate";
            }

            if (toDate != null)
            {
                sql += " AND pay.TransactionDate <= @ToDate";
            }

            sql += $" ORDER BY pay.RefNo DESC";
            //sql += $" AND req1.SaveBy = '{userID}' ORDER BY req1.RefNo DESC";




            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                if (!string.IsNullOrEmpty(req1RefNo))
                {
                    command.Parameters.AddWithValue("@ReqNo", req1RefNo);
                }

                if (fromDate != null)
                {
                    command.Parameters.AddWithValue("@FromDate", fromDate);
                }

                if (toDate != null)
                {
                    command.Parameters.AddWithValue("@ToDate", toDate);
                }

                using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                {
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    return dt;
                }
            }
        }
    }




    //=========================={ Item Grid OnRowDataBound }==========================
    protected void ItemGrid_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow && (e.Row.RowState & DataControlRowState.Edit) > 0)
        {
            // Set all row in edit mode
            //e.Row.RowState = e.Row.RowState ^ DataControlRowState.Edit;

            int rowIndex = e.Row.RowIndex;
            TextBox txtAvailableQty = (TextBox)e.Row.FindControl("txtAvailableQty");
            TextBox txtBalanceQty = (TextBox)e.Row.FindControl("txtBalanceQty");
            DataRowView rowView = (DataRowView)e.Row.DataItem;

            if (rowView != null)
            {
                // Retrieve AvailableQty value from the data source
                string availableQty = rowView["AvailableQty"].ToString();

                // Set the value to the TextBox
                txtAvailableQty.Text = availableQty;
            }
        }
    }




    //=========================={ Update - Fill Details }==========================

    protected void gridSearch_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        if (e.CommandName == "lnkView")
        {
            int rowId = Convert.ToInt32(e.CommandArgument);
            Session["Req1ReferenceNo"] = rowId;

            searchGridDiv.Visible = false;
            divTopSearch.Visible = false;
            UpdateDiv.Visible = true;

            FillHeaderDetails(rowId.ToString());

            FillItemDetails(rowId.ToString());

            FillTaxHead(rowId.ToString());

            // checking for exiting payment transactions
            FillPaymentTransaction(rowId.ToString());

            // fetching the payment remark if exists
            FillPaymentRemark(rowId.ToString());

            // checking for existing documents
            FillDocuments(rowId.ToString());
        }
    }

    private void FillHeaderDetails(string requisitionRefNo)
    {
        using (SqlConnection con = new SqlConnection(connectionString))
        {
            con.Open();

            // checking if the availability status is already done, so to make it non-editable
            DataTable existingBillDT = CheckForExistingBillingStatus(requisitionRefNo);

            if (existingBillDT.Rows.Count > 0)
            {
                // making buttons non-editable
                //btnSubmit.Enabled = false;

                getSweetAlertInfoMandatory("Work Order Issued", $"The Work Order Has Been Issued For Requisition: {requisitionRefNo}.");
            }

            string sql = $@"select * 
                            from ReqWorkOrder891 as wo 
                            inner join Requisition1891 req1 on wo.Req1RefNo = req1.RefNo 
                            INNER JOIN UserMaster891 as um ON req1.SaveBy = um.UserID 
                            INNER JOIN OrgType891 as org ON um.OrgType = org.RefID 
                            where req1.RefNo = @RefNo";

            SqlCommand cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@RefNo", requisitionRefNo);
            cmd.ExecuteNonQuery();

            SqlDataAdapter ad = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            ad.Fill(dt);


            Session["WorkOrderDT"] = (dt.Rows.Count > 0);

            // if for the 1st time ther wont be req1 data in workorder table, then take from paymentstatus table
            if (dt.Rows.Count > 0)
            {


                // 1st row
                txtReqNo.Text = dt.Rows[0]["ReqNo"].ToString();

                DateTime reqDate = DateTime.Parse(dt.Rows[0]["ReqDte"].ToString());
                dtReqDate.Text = reqDate.ToString("yyyy-MM-dd");

                // 2nd row
                OrgType.Text = dt.Rows[0]["OrgTyp"].ToString();
                InstituteName.Text = dt.Rows[0]["InstiName"].ToString();

                // 3rd row
                string workorderNo = dt.Rows[0]["WorkOrderNo"].ToString();
                if (workorderNo != "") WoNo.Text = workorderNo;
                else WoNo.Text = "System Generated";

                // workorder remark
                WoRemark.Value = dt.Rows[0]["WoRemark"].ToString();


                if (!string.IsNullOrEmpty(dt.Rows[0]["WorkOrderDate"].ToString()))
                {
                    DateTime woDatetxt = DateTime.Parse(dt.Rows[0]["WorkOrderDate"].ToString());
                    WoDate.Text = woDatetxt.ToString("yyyy-MM-dd");
                }
                else
                {
                    WoDate.Text = DateTime.Now.ToString("dd-MM-yyyy");
                }
            }
            else
            {
                string sql_new = $@"select * 
                                    from Requisition1891 as req1 
                                    INNER JOIN UserMaster891 as um ON req1.SaveBy = um.UserID 
                                    INNER JOIN OrgType891 as org ON um.OrgType = org.RefID 
                                    where req1.RefNo = @RefNo";

                SqlCommand cmd_New = new SqlCommand(sql_new, con);
                cmd_New.Parameters.AddWithValue("@RefNo", requisitionRefNo);
                cmd_New.ExecuteNonQuery();

                SqlDataAdapter ad_New = new SqlDataAdapter(cmd_New);
                DataTable dt_New = new DataTable();
                ad_New.Fill(dt_New);
                con.Close();

                if (dt_New.Rows.Count > 0)
                {
                    // 1st row
                    txtReqNo.Text = dt_New.Rows[0]["ReqNo"].ToString();

                    DateTime reqDate = DateTime.Parse(dt_New.Rows[0]["ReqDte"].ToString());
                    dtReqDate.Text = reqDate.ToString("yyyy-MM-dd");

                    // 2nd row
                    OrgType.Text = dt_New.Rows[0]["OrgTyp"].ToString();
                    InstituteName.Text = dt_New.Rows[0]["InstiName"].ToString();

                    // 3rd row
                    WoNo.Text = "System Generated";
                    WoDate.Text = DateTime.Now.ToString("dd-MM-yyyy");
                }
            }

            con.Close();
        }
    }

    private void FillItemDetails(string requisitionRefNo)
    {
        itemDiv.Visible = true;

        using (SqlConnection con = new SqlConnection(connectionString))
        {
            con.Open();

            string sql = $@"select req2.*, service.ServName, uom.umName 
                            from Requisition2891 as req2 
                            INNER JOIN ServMaster891 as service ON req2.ServiceName = service.RefID 
                            INNER JOIN UOMs891 as uom ON req2.UOM = uom.UmId 
                            INNER JOIN ReqReceived891 as reqrec ON req2.RefNo = reqrec.Req2RefNo 
                            where req2.BillRefNo = @BillRefNo";

            SqlCommand cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@BillRefNo", requisitionRefNo);
            cmd.ExecuteNonQuery();

            SqlDataAdapter ad = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            ad.Fill(dt);


            // manually adding new custom editable column
            dt.Columns.Add("AvailableQty", typeof(double));
            dt.Columns.Add("BalanceQty", typeof(double));

            foreach (DataRow row in dt.Rows)
            {
                row["AvailableQty"] = 0;
            }

            // adding the new column with checkboxes
            DataColumn checkboxColumn = new DataColumn("AvailableStatus", typeof(bool));
            checkboxColumn.DefaultValue = false;
            dt.Columns.Add(checkboxColumn);


            con.Close();

            itemGrid.DataSource = dt;
            itemGrid.DataBind();

            CheckingForCheckboxStatus(dt);

            ViewState["ReqDetailsVS"] = dt;
            Session["ReqDetails"] = dt;

            // total service amount
            //double? totalServiceAmount = dt.AsEnumerable().Sum(row => row["ServicePrice"] is DBNull ? (double?)null : Convert.ToDouble(row["ServicePrice"])) ?? 0.0;
            //TotalServiceAmnt.Text = totalServiceAmount.HasValue ? totalServiceAmount.Value.ToString("N2") : "0.00";
        }
    }

    private void CheckingForCheckboxStatus(DataTable Requisition2DT)
    {
        DataTable ReqReceivedDT = new DataTable();

        foreach (DataRow row in Requisition2DT.Rows)
        {
            string requisition2Refno = row["RefNo"].ToString();

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string sql = "SELECT * FROM ReqReceived891 WHERE Req2RefNo = @Req2RefNo";

                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@Req2RefNo", requisition2Refno);
                con.Open();

                SqlDataAdapter ad = new SqlDataAdapter(cmd);
                ad.Fill(ReqReceivedDT);
            }

            // checking prechecked checkbox
            foreach (DataRow reqReceivedRow in ReqReceivedDT.Rows)
            {
                // calculating balance qty for each row and assigning the value
                row["BalanceQty"] = Convert.ToDouble(reqReceivedRow["ReqQty"]) - Convert.ToDouble(reqReceivedRow["AvailableQty"]);

                bool availableStatus = Convert.ToBoolean(reqReceivedRow["AvailableStatus"]);

                TextBox txtEdit = (TextBox)itemGrid.Rows[Requisition2DT.Rows.IndexOf(row)].FindControl("txtEdit");
                txtEdit.Text = reqReceivedRow["NonAvailabilityRemark"].ToString();

                // code for html input (type checkbox)
                //HtmlInputCheckBox chkAvailStatus = (HtmlInputCheckBox)itemGrid.Rows[Requisition2DT.Rows.IndexOf(row)].FindControl("chkAvailStatus");

                // code for asp:checkbox
                //CheckBox chkAvailStatus = (CheckBox)itemGrid.Rows[Requisition2DT.Rows.IndexOf(row)].FindControl("chkAvailStatus"); // asp:checkbox
                HtmlInputCheckBox chkAvailStatus = (HtmlInputCheckBox)itemGrid.Rows[Requisition2DT.Rows.IndexOf(row)].FindControl("chkAvailStatus"); // input - checkbox
                if (chkAvailStatus != null)
                {
                    // Use Checked property of asp:CheckBox
                    chkAvailStatus.Checked = availableStatus;
                    txtEdit.Visible = !availableStatus;
                }
            }
        }

        if (ReqReceivedDT.Rows.Count > 0)
        {
            // calculating total aervice amount if checkboxs are pre-checked
            double totalBill = 0.00;
            foreach (GridViewRow row in itemGrid.Rows)
            {
                int rowIndex = row.RowIndex;

                double requiredQty = Convert.ToDouble(ReqReceivedDT.Rows[rowIndex]["ReqQty"]);
                double availableQty = Convert.ToDouble(ReqReceivedDT.Rows[rowIndex]["AvailableQty"]);
                double servicePrice = Convert.ToDouble(ReqReceivedDT.Rows[rowIndex]["ServicePrice"]);

                // to get actual previously entered available qty values
                TextBox TextAvailableQty = row.FindControl("txtAvailableQty") as TextBox;
                TextAvailableQty.Text = availableQty.ToString();
                //TextAvailableQty.Text = "0";

                // checking the balance qty
                TextBox txtBalanceQty = row.FindControl("txtBalanceQty") as TextBox;
                txtBalanceQty.Text = (requiredQty - availableQty).ToString();

                // code for html input (type checkbox)
                string availableStatus = ((HtmlInputCheckBox)row.FindControl("chkAvailStatus")).Checked.ToString(); // input - checkbox

                // code for asp:checkbox
                //CheckBox chkAvailStatus = (CheckBox)row.FindControl("chkAvailStatus");
                //string availableStatus = chkAvailStatus.Checked.ToString();



                if (availableStatus == "True")
                {
                    totalBill = totalBill + (availableQty * servicePrice);
                }
            }

            TotalServiceAmnt.Text = totalBill.ToString("N2");
            Session["TotalBillAmount"] = totalBill;
        }
    }


    private void FillPaymentTransaction(string req1RefNo)
    {
        using (SqlConnection con = new SqlConnection(connectionString))
        {
            con.Open();

            string sql = $@"select pay.*, tds.TdsSection as tdsSectionOg, tds.TdsRate, tds.PayNature, tds.RefID  
                            from PaymentStatus891 as pay
                            inner join TdsSection891 as tds on pay.TdsRefID = tds.RefID 
                            where pay.Req1RefNo = @Req1RefNo";

            SqlCommand cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@Req1RefNo", req1RefNo);
            cmd.ExecuteNonQuery();

            SqlDataAdapter ad = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            ad.Fill(dt);

            con.Close();

            if (dt.Rows.Count > 0)
            {
                // full payment done

                PaymentGrid.DataSource = dt;
                PaymentGrid.DataBind();

                ViewState["ItemDetails_VS"] = dt;
                Session["ItemDetails"] = dt;

                // was TDS applied ?
                bool tdsCheckBoxStatus = Convert.ToBoolean(dt.Rows[0]["TdsCheckStatus"].ToString());
                AppplyTDS.Checked = tdsCheckBoxStatus;

                if (AppplyTDS.Checked)
                {
                    tdsDiv.Visible = true;

                    // autofilling tds details

                    TDSSection.DataSource = dt;
                    TDSSection.DataTextField = "tdsSectionOg";
                    TDSSection.DataValueField = "tdsSectionOg";
                    TDSSection.DataBind();

                    TDSSection.Items.Insert(0, new ListItem("------Select TDS Section------", "0"));
                    if (TDSSection.SelectedIndex < 2) TDSSection.SelectedIndex = 1;

                    PaymentNature.Value = dt.Rows[0]["TdsPayNature"].ToString();
                    TDSRate.Text = dt.Rows[0]["TdsRate"].ToString();

                    // re-calculating the payment amount based on this selected tds section
                    string tdsSectionRefID = dt.Rows[0]["RefID"].ToString();

                    CalculateTDS(tdsSectionRefID);
                }
            }
        }
    }


    private void FillPaymentRemark(string req1RefNo)
    {
        using (SqlConnection con = new SqlConnection(connectionString))
        {
            con.Open();

            //string sql = $@"select * from Requisition1891 where RefNo = @RefNo";

            string sql = $@"select * from Requisition1891 where RefNo = @RefNo";

            SqlCommand cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@RefNo", req1RefNo);
            cmd.ExecuteNonQuery();

            SqlDataAdapter ad = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            ad.Fill(dt);
            con.Close();


            // availability remark
            AvailabilityRemark.Value = dt.Rows[0]["AvailabilityRemark"].ToString();

            // customer remark
            RemarkInput.Value = dt.Rows[0]["PaymentRemark"].ToString();
        }
    }

    private void FillDocuments(string req1RefNo)
    {
        using (SqlConnection con = new SqlConnection(connectionString))
        {
            con.Open();
            string sql = "select * from BillDocUpload891 where BillRefNo = @BillRefNo AND DocCategory = 'Payment'";
            SqlCommand cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@BillRefNo", req1RefNo);
            cmd.ExecuteNonQuery();

            SqlDataAdapter ad = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            ad.Fill(dt);
            con.Close();

            if (dt.Rows.Count > 0)
            {
                docGrid.Visible = true;

                GridDocument.DataSource = dt;
                GridDocument.DataBind();

                ViewState["DocDetails_VS"] = dt;
                Session["DocUploadDT"] = dt;
            }
        }
    }




    //=========================={ GridView RowDeleting }==========================

    protected void Grid_RowDeleting(object sender, GridViewDeleteEventArgs e)
    {
        GridView gridView = (GridView)sender;

        // item gridview
        //if (gridView.ID == "itemGrid")
        //{
        //    int rowIndex = e.RowIndex;

        //    DataTable dt = ViewState["ItemDetails_VS"] as DataTable;

        //    if (dt != null && dt.Rows.Count > rowIndex)
        //    {
        //        dt.Rows.RemoveAt(rowIndex);

        //        ViewState["ItemDetails_VS"] = dt;
        //        Session["ItemDetails"] = dt;

        //        itemGrid.DataSource = dt;
        //        itemGrid.DataBind();

        //        // re-calculating total amount n assigning back to textbox
        //        double? totalBillAmount = dt.AsEnumerable().Sum(row => row["Amount"] is DBNull ? (double?)null : Convert.ToDouble(row["Amount"])) ?? 0.0;
        //        txtBillAmount.Text = totalBillAmount.HasValue ? totalBillAmount.Value.ToString("N2") : "0.00";

        //        // re-calculating taxes
        //        //FillTaxHead();
        //    }
        //}

        // document gridview
        //if (gridView.ID == "GridDocument")
        //{
        //    int rowIndex = e.RowIndex;

        //    DataTable dt = ViewState["DocDetails_VS"] as DataTable;

        //    if (dt != null && dt.Rows.Count > rowIndex)
        //    {
        //        dt.Rows.RemoveAt(rowIndex);

        //        ViewState["DocDetails_VS"] = dt;
        //        Session["DocUploadDT"] = dt;

        //        GridDocument.DataSource = dt;
        //        GridDocument.DataBind();
        //    }
        //}
    }




    //=========================={ Tax Head }==========================
    protected void GridTax_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow && (e.Row.RowState & DataControlRowState.Edit) > 0)
        {
            // Set the row in edit mode
            e.Row.RowState = e.Row.RowState ^ DataControlRowState.Edit;
        }

        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            string req1RefNo = Session["Req1ReferenceNo"].ToString();

            // fetching acount head or taxes
            DataTable accountHeadExistingDT = getAccountHeadExisting(req1RefNo);


            //=================================={ Add/Less column }========================================
            DropDownList ddlAddLess = (DropDownList)e.Row.FindControl("AddLess");

            if (ddlAddLess != null)
            {
                string addLessValue = accountHeadExistingDT.Rows[e.Row.RowIndex]["AddLess"].ToString();
                ddlAddLess.SelectedValue = addLessValue;
            }

            //=================================={ Percentage/Amount column }========================================
            DropDownList ddlPerOrAmnt = (DropDownList)e.Row.FindControl("PerOrAmnt");

            if (ddlPerOrAmnt != null)
            {
                string perOrAmntValue = accountHeadExistingDT.Rows[e.Row.RowIndex]["PerOrAmnt"].ToString();
                ddlPerOrAmnt.SelectedValue = perOrAmntValue;
            }
        }
    }

    private void FillTaxHead(string req1RefNo)
    {
        DataTable accountHeadExistingDT = getAccountHeadExisting(req1RefNo);

        if (accountHeadExistingDT.Rows.Count > 0)
        {
            double totalBillAmount = Convert.ToDouble(Session["TotalBillAmount"]);

            Session["AccountHeadDT"] = accountHeadExistingDT;
            autoFilltaxHeads(accountHeadExistingDT, totalBillAmount);
        }
        else
        {
            // if tax was not applied at availability status

            divTaxHead.Visible = false;

            double basicServiceAmount = Convert.ToDouble(Session["TotalBillAmount"]);
            txtNetAmnt.Text = basicServiceAmount.ToString("N2");

            PaymentAmount.Text = Math.Floor(basicServiceAmount).ToString("");
            RightOffAmount.Text = (basicServiceAmount % 1).ToString("N2");
        }
    }

    private void autoFilltaxHeads(DataTable accountHeadDT, double bscAmnt)
    {
        double basicAmount = bscAmnt;
        double totalDeduction = 0.00;
        double totalAddition = 0.00;
        double netAmount = 0.00;

        foreach (DataRow row in accountHeadDT.Rows)
        {
            double percentage = Convert.ToDouble(row["TaxRate"]);

            double factorInPer = (basicAmount * percentage) / 100;

            if (row["AddLess"].ToString() == "Add")
            {
                totalAddition = totalAddition + factorInPer;
            }
            else
            {
                totalDeduction = totalDeduction + factorInPer;
            }

            row["TaxAmount"] = factorInPer;
        }

        GridTax.DataSource = accountHeadDT;
        GridTax.DataBind();

        // filling total deduction
        txtTotalDeduct.Text = Math.Abs(totalDeduction).ToString("N2");

        // filling total addition
        txtTotalAdd.Text = totalAddition.ToString("N2");

        // Net Amount after tax deductions or addition
        netAmount = (basicAmount + totalAddition) - Math.Abs(totalDeduction);
        txtNetAmnt.Text = netAmount.ToString("N2");

        // initially net amount == payment amount
        PaymentAmount.Text = Math.Floor(netAmount).ToString("N2");
        RightOffAmount.Text = (netAmount % 1).ToString("N2");
    }



    //=========================={ Apply TDS }==========================
    protected void ApplyTDS_CheckedChanged(object sender, EventArgs e)
    {
        CheckBox checkBox = (CheckBox)sender;
        bool isChecked = checkBox.Checked;

        if (isChecked)
        {
            tdsDiv.Visible = true;
            tdsAmountTextbox.Visible = true;
        }
        else
        {
            tdsDiv.Visible = false;
            tdsAmountTextbox.Visible = false;
            TDSAmount.Text = (0).ToString();

            // re-calculating tds and payment amount == 0
            CalculateTDS(string.Empty);
        }
    }






    //=========================={ Submit Button Click Event }==========================
    protected void btnBack_Click(object sender, EventArgs e)
    {
        Response.Redirect("WorkOrder.aspx");
    }

    protected void btnSubmit_Click(object sender, EventArgs e)
    {
        if (GridDocument.Rows.Count > 0)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlTransaction transaction = con.BeginTransaction();

                try
                {
                    string req1RefNo = txtReqNo.Text.ToString();

                    // save or update work order details
                    SaveWorkOrderDetails(req1RefNo, con, transaction);

                    if (transaction.Connection != null) transaction.Commit();
                }
                catch (Exception ex)
                {
                    getSweetAlertErrorMandatory("Oops!", $"{ex.Message}");
                    transaction.Rollback();
                }
                finally
                {
                    con.Close();
                    transaction.Dispose();
                }
            }
        }
        else
        {
            getSweetAlertErrorMandatory("No Document Found", $"Kindly Minimum 1 Payment Related Document");
        }


    }

    private void SaveWorkOrderDetails(string req1RefNo, SqlConnection con, SqlTransaction transaction)
    {
        string userID = Session["UserId"].ToString();

        string workOrderNo = WoNo.Text.ToString();
        DateTime workOderDate = DateTime.Parse(WoDate.Text);

        DateTime req1Date = DateTime.Parse(dtReqDate.Text);

        // workorder remark
        string workOrderRemark = WoRemark.Value;


        // checking for existing workorder datatable
        bool workOrderExists = (bool)Session["WorkOrderDT"];

        if (workOrderExists)
        {
            // update

            string sql = $@"UPDATE ReqWorkOrder891 SET WoRemark = @WoRemark, WorkOrderDate=@WorkOrderDate where RefNo = @RefNo";

            SqlCommand cmd = new SqlCommand(sql, con, transaction);
            cmd.Parameters.AddWithValue("@WoRemark", workOrderRemark);
            cmd.Parameters.AddWithValue("@WorkOrderDate", workOderDate);
            cmd.Parameters.AddWithValue("@RefNo", req1RefNo);
            int k = cmd.ExecuteNonQuery();

            //SqlDataAdapter ad = new SqlDataAdapter(cmd);
            //DataTable dt = new DataTable();
            //ad.Fill(dt);

            if (k > 0)
            {
                getSweetAlertSuccessRedirectMandatory("Work Order Updated!", $"The Work Order: {workOrderNo}, For Requisition No: {req1RefNo} Updated Successfully", "WorkOrder.aspx");
            }
        }
        else
        {
            // insert new workorder

            // new workorder ref no
            string workOrderRefNo_New = GetNewWorkOrderRefNo(con, transaction);

            Session["WorOrderNumber"] = workOrderRefNo_New;

            string sql = $@"INSERT INTO ReqWorkOrder891 
                            (RefNo, Req1RefNo, ReqDate, WorkOrderNo, WorkOrderDate, WoRemark, SaveBy) 
                            values 
                            (@RefNo, @Req1RefNo, @ReqDate, @WorkOrderNo, @WorkOrderDate, @WoRemark, @SaveBy)";

            SqlCommand cmd = new SqlCommand(sql, con, transaction);
            cmd.Parameters.AddWithValue("@RefNo", workOrderRefNo_New);
            cmd.Parameters.AddWithValue("@Req1RefNo", req1RefNo);
            cmd.Parameters.AddWithValue("@ReqDate", req1Date);
            cmd.Parameters.AddWithValue("@WorkOrderNo", workOrderRefNo_New);
            cmd.Parameters.AddWithValue("@WorkOrderDate", workOderDate);
            cmd.Parameters.AddWithValue("@WoRemark", workOrderRemark);
            cmd.Parameters.AddWithValue("@SaveBy", userID);
            int k = cmd.ExecuteNonQuery();

            //SqlDataAdapter ad = new SqlDataAdapter(cmd);
            //DataTable dt = new DataTable();
            //ad.Fill(dt);

            if (k > 0)
            {
                getSweetAlertSuccessRedirectMandatory("Work Order Saved!", $"The Work Order: {workOrderRefNo_New}, For Requisition No: {req1RefNo} Saved Successfully", "WorkOrder.aspx");
            }
        }
    }

}